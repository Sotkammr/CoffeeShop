using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoffeeShop.Classes
{
    public class DatabaseHelper
    {
        private static DatabaseHelper _instance;
        public static DatabaseHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DatabaseHelper();
                return _instance;
            }
        }
        private string connectionString;

        private DatabaseHelper()
        {
            try
            {
                connectionString = ConfigurationManager.ConnectionStrings["CoffeeShopDB"].ConnectionString;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка чтения строки подключения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public SqlConnection GetOpenConnection()
        {
            var conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
        }
        public bool TestConnection()
        {
            try
            {
                using (var conn = GetOpenConnection())
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}\n\n" +
                               "Проверьте:\n" +
                               "1. Запущен ли SQL Server (DESKTOP-TKS0ILD\\SQLEXPRESS)\n" +
                               "2. Создана ли база данных CoffeeShopDB",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}