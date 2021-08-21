using System;
using System.Collections.Generic;
using System.Windows;

namespace DatabaseBuddy.Core.OperationVisualization
{
    public class SuppressDependencyPropertyChangedEventScope : IDisposable
    {
        private static Dictionary<DependencyProperty, int> m_Properties = new Dictionary<DependencyProperty, int>();

        public static bool SuppressChangedEvent(DependencyProperty Property)
        {
            return m_Properties.ContainsKey(Property);
        }

        private DependencyProperty m_Property;

        public SuppressDependencyPropertyChangedEventScope(DependencyProperty Property)
        {
            if (!m_Properties.ContainsKey(Property))
                m_Properties.Add(Property, 0);

            m_Properties[Property]++;
            m_Property = Property;
        }

        public void Dispose()
        {
            m_Properties[m_Property]--;
            if (m_Properties[m_Property] == 0)
                m_Properties.Remove(m_Property);
        }
    }
}