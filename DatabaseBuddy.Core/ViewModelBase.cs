using DatabaseBuddy.Core.Attributes;
using DatabaseBuddy.Core.Events;
using DatabaseBuddy.Core.Extender;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DatabaseBuddy.Core
{
    public abstract class ViewModelBase
    {
        #region events

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ExecuteCommandRequestedEventArgs> ExecuteCommandRequested;

        #endregion

        private enum eMemberInfoCacheType
        {
            PropertyMap,
            CanExecuteMap,
            MethodMap,
            ExecuteMap
        }

        #region needs

        private const string EXECUTE_PREFIX = "Execute_";
        private const string CAN_EXECUTE_PREFIX = "CanExecute_";
        private bool m_disposed;
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        private static readonly ConcurrentDictionary<Type, IDictionary<eMemberInfoCacheType, IDictionary<string, List<string>>>> m_MemberInfoCache = new();
        private static readonly ConcurrentDictionary<Type, IDictionary<string, PropertyInfo>> m_PropertyInfoCache = new();
        private static readonly ConcurrentDictionary<Type, IDictionary<string, MethodInfo>> m_MethodInfoCache = new();

        private readonly IDictionary<string, List<string>> _propertyMap;
        private readonly IDictionary<string, List<string>> _methodMap;
        private readonly IDictionary<string, List<string>> _canExecuteMap;
        private readonly IEnumerable<string> _CommandNames;
        private readonly IDictionary<string, PropertyInfo> _MyProperies;
        private readonly IDictionary<string, MethodInfo> _MyMethods;


        #endregion

        #region ctor / dtor


        public ViewModelBase()
        {

            Type MyType = GetType();

            m_PropertyInfoCache.TryGetValue(MyType, out IDictionary<string, PropertyInfo> PropertyInfos);
            if (PropertyInfos == null)
            {
                PropertyInfos = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);

                foreach (var pi in MyType.GetProperties())
                {
                    PropertyInfos[pi.Name] = pi;
                }
                m_PropertyInfoCache.TryAdd(MyType, PropertyInfos);
            }
            _MyProperies = PropertyInfos;

            var ExecuteList = new List<MemberInfo>();
            var NotExecuteList = new List<MemberInfo>();
            var CommandNames = new List<string>();

            m_MethodInfoCache.TryGetValue(MyType, out IDictionary<string, MethodInfo> MethodInfos);
            if (MethodInfos == null)
            {
                MethodInfos = new Dictionary<string, MethodInfo>(StringComparer.InvariantCultureIgnoreCase);

                foreach (var MemberInf in MyType.GetMethods())
                {
                    if (MemberInf.Name.StartsWith(CAN_EXECUTE_PREFIX))
                        ExecuteList.Add(MemberInf);
                    else
                        NotExecuteList.Add(MemberInf);

                    if (MemberInf.Name.StartsWith(EXECUTE_PREFIX))
                        CommandNames.Add(MemberInf.Name.StripLeft(EXECUTE_PREFIX.Length));

                    MethodInfos[MemberInf.Name] = MemberInf;
                    MethodInfos[__BuildMethodNameWithParameters(MemberInf)] = MemberInf;
                }
                m_MethodInfoCache.TryAdd(MyType, MethodInfos);
            }
            _MyMethods = MethodInfos;

            m_MemberInfoCache.TryGetValue(MyType, out IDictionary<eMemberInfoCacheType, IDictionary<string, List<string>>> MemberInfoCache);
            if (MemberInfoCache == null)
            {
                MemberInfoCache = new Dictionary<eMemberInfoCacheType, IDictionary<string, List<string>>>();

                var CommandNamesDict = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
                CommandNamesDict[EXECUTE_PREFIX] = CommandNames;

                MemberInfoCache[eMemberInfoCacheType.CanExecuteMap] = MapDependencies(ExecuteList, false);
                MemberInfoCache[eMemberInfoCacheType.ExecuteMap] = CommandNamesDict;
                MemberInfoCache[eMemberInfoCacheType.MethodMap] = MapDependencies(NotExecuteList, true);
                MemberInfoCache[eMemberInfoCacheType.PropertyMap] = MapDependencies(_MyProperies.Values, false);
                m_MemberInfoCache.TryAdd(MyType, MemberInfoCache);
            }
            _canExecuteMap = MemberInfoCache[eMemberInfoCacheType.CanExecuteMap];
            _methodMap = MemberInfoCache[eMemberInfoCacheType.MethodMap];
            _propertyMap = MemberInfoCache[eMemberInfoCacheType.PropertyMap];
            _CommandNames = MemberInfoCache[eMemberInfoCacheType.ExecuteMap][EXECUTE_PREFIX];

            //GenerateFieldProperties();
        }

        #endregion

        #region properties

        public bool IsExecuteCommandRequestedAttached
        {
            get
            {
                return ExecuteCommandRequested != null;
            }
        }

        public bool IsDisposed
        {
            get { return m_disposed; }
            private set { m_disposed = value; }
        }

        #endregion

        #region - public methods -

        #endregion

        #region on event methods

        #region [OnPropertyChanged]
        protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            __OnPropertyChanged(PropertyName);
        }


        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> expression)
        {
            OnPropertyChanged(GetPropertyName(expression));
        }


        #endregion

        #region [ForcePropertyChanged]
        protected virtual void ForcePropertyChanged([CallerMemberName] string PropertyName = null)
        {
            __OnPropertyChanged(PropertyName);
        }
        #endregion

        protected virtual void OnExecuteCommandRequested(string CommandName, Object CommandParameters)
        {
            if (ExecuteCommandRequested != null)
                ExecuteCommandRequested(this, new ExecuteCommandRequestedEventArgs(CommandName, CommandParameters));
        }

        #endregion

        #region helper
        #region - private methods -

        private IDictionary<string, List<string>> MapDependencies(IEnumerable<MemberInfo> MemberInfos, bool useParametersAsKey)
        {
            IDictionary<string, List<string>> Result = new Dictionary<string, List<string>>();
            foreach (var MemberInfo in MemberInfos)
            {
                try
                {
                    string Key = MemberInfo is MethodInfo && useParametersAsKey ? __BuildMethodNameWithParameters(MemberInfo as MethodInfo) : MemberInfo.Name;

                    Result.Add(Key, MemberInfo.GetCustomAttributes(typeof(DependsUponAttribute), true)
                                    .Cast<DependsUponAttribute>()
                                    .Select(a => a.DependencyName)
                                    .ToList());
                }
                catch (Exception ex)
                {
                    throw new Exception("Error mapping dependency: " + MemberInfo.Name + " - " + ex.ToString());
                }
            }

            return Invert(Result);
        }

        #region [__OnPropertyChanged]
        private void __OnPropertyChanged(string PropertyName)
        {
            PropertyChanged.Raise(this, PropertyName);

            RaiseCanExecuteChanges(PropertyName);
        }
        #endregion

        private void GenerateCommands()
        {
            foreach (var CommandName in _CommandNames)
            {
                /*Set(CommandName, new RelayCommand(x => ExecuteCommand(CommandName, x),*/
                CanExecuteCommand(CommandName, CommandName);
            }
        }
        private bool IsRealProperty(string PropertyName)
        {
            return _MyProperies.ContainsKey(PropertyName);
        }

        private void ExecuteCommand(string name, object parameter)
        {
            MethodInfo methodInfo;
            _MyMethods.TryGetValue(EXECUTE_PREFIX + name, out methodInfo);
            if (methodInfo == null) return;

            methodInfo.Invoke(this, methodInfo.GetParameters().Length == 1 ? new[] { parameter } : null);
        }

        private bool CanExecuteCommand(string name, object parameter)
        {

            MethodInfo methodInfo;
            _MyMethods.TryGetValue(CAN_EXECUTE_PREFIX + name, out methodInfo);
            if (methodInfo == null) return true;

            return (bool)methodInfo.Invoke(this, methodInfo.GetParameters().Length == 1 ? new[] { parameter } : null);
        }

        private string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
                throw new ArgumentException($"{nameof(expression)} must be a property {nameof(expression)}");

            return memberExpression.Member.Name;
        }

        private IEnumerable<PropertyInfo> GetPropertiesWithAttribute(Type AttributeType)
        {
            return GetType().GetProperties().Where(p => p.GetCustomAttributes(AttributeType, true).Length > 0);
        }



        private IDictionary<string, List<string>> Invert(IDictionary<string, List<string>> map)
        {
            var flattened = from key in map.Keys
                            from value in map[key]
                            select new { Key = key, Value = value };

            var uniqueValues = flattened.Select(x => x.Value).Distinct();

            return uniqueValues.ToDictionary(
                        x => x,
                        x => (from item in flattened
                              where item.Value == x
                              select item.Key).ToList());
        }

        private string __BuildMethodNameWithParameters(MethodInfo method)
        {
            var Result = new StringBuilder(method.Name);

            foreach (var Parameter in method.GetParameters())
                Result.Append(Parameter.ParameterType.Name);

            return Result.ToString();
        }


        private void ExecuteMethod(string MethodName)
        {
            MethodInfo methodInfo;
            _MyMethods.TryGetValue(MethodName, out methodInfo);
            if (methodInfo == null)
                return;

            methodInfo.Invoke(this, null);
        }
        #endregion

        protected void RaiseCanExecuteChangedEvent(string canExecute_name)
        {
            string commandName = canExecute_name.StripLeft(CAN_EXECUTE_PREFIX.Length);

            RelayCommand Command = null;
            //if (AutoGenerateCommands)
            //{
            //    //Command = Get<RelayCommand>(commandName);
            //}
            //else
            //{
            PropertyInfo PI;
            _MyProperies.TryGetValue(commandName, out PI);
            if (PI != null)
                Command = PI.GetValue(this, new object[] { }) as RelayCommand;
            //}

            if (Command == null)
                return;

            Command.OnCanExecuteChanged();
        }

        private void RaiseCanExecuteChanges(string PropertyName)
        {
            if (_canExecuteMap.ContainsKey(PropertyName))
                _canExecuteMap[PropertyName].ForEach(RaiseCanExecuteChangedEvent);
        }

        #endregion

        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || IsDisposed)
                return;

            IsDisposed = true;


            if (_values != null)
                _values.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion     

        #region [GetPropertyNames]
        public ICollection<string> GetPropertyNames()
        {
            IDictionary<string, PropertyInfo> PropertyInfos;

            m_PropertyInfoCache.TryGetValue(this.GetType(), out PropertyInfos);
            if (PropertyInfos.IsNull())
                return new List<string>();
            return PropertyInfos.Keys;
        }
        #endregion
    }
}
