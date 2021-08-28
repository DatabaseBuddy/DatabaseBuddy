using DatabaseBuddy.Core.Extender;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DatabaseBuddy.Core.DatabaseExtender
{
    public class DBConnection
    {
        private SqlConnection m_Connection;
        private SqlCommand m_Command;
        private readonly IDictionary<string, object> m_Parameters = new Dictionary<string, object>();
        private string m_ServerName;
        private string m_DatabaseName;
        private string m_Username;
        private string m_Password;
        private bool m_IntegratedSecurity;
        public event EventHandler ConnectionFailed;

        public DBConnection(string ServerName, string DatabaseName,
            string Username, string Password)
        {
            m_ServerName = ServerName;
            m_DatabaseName = DatabaseName;
            m_Username = Username;
            m_Password = Password;
            __GetConnection();
        }

        public DBConnection(string ServerName, string DatabaseName, bool IntegratedSecurity)
        {
            m_ServerName = ServerName;
            m_DatabaseName = DatabaseName;
            m_IntegratedSecurity = IntegratedSecurity;
            __GetConnection();
        }
        #region GetConnection
        private SqlConnection __GetConnection()
        {
            var SqlConnBuilder = new SqlConnectionStringBuilder
            {
                DataSource = m_ServerName,
                InitialCatalog = m_DatabaseName,
            };
            if (!m_IntegratedSecurity)
            {
                SqlConnBuilder.UserID = m_Username;
                SqlConnBuilder.Password = m_Password;
            }
            else
                SqlConnBuilder.IntegratedSecurity = true;
            m_Connection = new SqlConnection(SqlConnBuilder.ToString());
            return m_Connection;
        }
        #endregion

        public object ExecuteScalar(string CommandText)
        {
            try
            {
                if (m_Connection.State == System.Data.ConnectionState.Closed)
                    m_Connection.Open();
                m_Command = new SqlCommand(CommandText, m_Connection);
                __AddParameters();
                return m_Command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                __ResetConnection(ex);
                return ex.ToString();
            }
            finally
            {
                m_Parameters.Clear();
                m_Connection.Close();
            }
        }

        public object ExecuteNonQuery(string CommandText)
        {
            try
            {
                if (m_Connection.State == System.Data.ConnectionState.Closed)
                    m_Connection.Open();
                m_Command = new SqlCommand(CommandText, m_Connection);
                __AddParameters();
                return m_Command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                __ResetConnection(ex);
                return ex.ToString();
            }
            finally
            {
                m_Parameters.Clear();
                m_Connection.Close();
            }
        }

        public SqlDataReader GetDataReader(string CommandText)
        {
            try
            {
                if (m_Connection.State == System.Data.ConnectionState.Closed)
                    m_Connection.Open();
                m_Command = new SqlCommand(CommandText, m_Connection);
                __AddParameters();
                return m_Command.ExecuteReader();
            }
            catch (Exception ex)
            {
                __ResetConnection(ex);
                return null;
            }
            finally
            {
                m_Parameters.Clear();
            }
        }

        public void CloseDataReader()
        {
            m_Connection.Close();
        }

        public void AddParameter(string ParameterName, object value)
        {
            m_Parameters.Add(ParameterName, value);
        }

        private void __AddParameters()
        {
            foreach (var Parameter in m_Parameters)
            {
                m_Command.Parameters.Add(new SqlParameter
                {
                    ParameterName = Parameter.Key,
                    Value = Parameter.Value,
                });
            }
        }

        private void __ResetConnection(Exception ex)
        {
            if (ex is SqlException SqlEx)
            {
                ConnectionFailed?.Invoke(SqlEx, new EventArgs());
            }
        }

    }
}
