using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Infrastructure
{
    public sealed class ConnectionManager
    {
        private static SqlConnection _connection;
        private static readonly object _lock = new();

        private ConnectionManager() { }

        public static SqlConnection GetConnection()
        {
            if (_connection == null || _connection.State == System.Data.ConnectionState.Closed)
            {
                lock (_lock)
                {
                    if (_connection == null || _connection.State == System.Data.ConnectionState.Closed)
                    {
                        _connection = new SqlConnection(AppSettings.ConnectionString);
                        _connection.Open();
                    }
                }
            }
            return _connection;
        }

        public static void CloseConnection()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
    }
    public sealed class ConnectionManager
    {
        private static SqlConnection _connection;
        private static readonly object _lock = new();

        private ConnectionManager() { }

        public static SqlConnection GetConnection()
        {
            if (_connection == null || _connection.State == System.Data.ConnectionState.Closed)
            {
                lock (_lock)
                {
                    if (_connection == null || _connection.State == System.Data.ConnectionState.Closed)
                    {
                        _connection = new SqlConnection(AppSettings.ConnectionString);
                        _connection.Open();
                    }
                }
            }
            return _connection;
        }

        public static void CloseConnection()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
