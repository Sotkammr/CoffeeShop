using CoffeeShopSales.Classes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoffeeShop.Classes
{
    public class DataManager
    {
        private static DataManager _instance;
        public static DataManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DataManager();
                return _instance;
            }
        }
        public string CurrentUser { get; set; }
        public User CurrentUserInfo { get; set; }

        private DataManager() { }
        public bool AuthenticateUser(string login, string password)
        {
            try
            {
                using (var conn = DatabaseHelper.Instance.GetOpenConnection())
                {
                    string query = @"
                        SELECT Id_Employee, Login, FullName, Position 
                        FROM View_UserAuth 
                        WHERE Login = @login AND Password = @password";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                CurrentUser = login;
                                CurrentUserInfo = new User
                                {
                                    Id = reader.GetInt32(0),
                                    Login = reader.GetString(1),
                                    FullName = reader.GetString(2),
                                    Position = reader.GetString(3)
                                };
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}");
            }
            return false;
        }
        public User GetCurrentUser()
        {
            return CurrentUserInfo;
        }
        public User GetUserByLogin(string login)
        {
            try
            {
                using (var conn = DatabaseHelper.Instance.GetOpenConnection())
                {
                    string query = @"
                        SELECT e.Id_Employee, a.Login, a.Password, 
                               e.Surname + ' ' + e.Name + ' ' + ISNULL(e.Patronymic, '') AS FullName,
                               p.Name AS Position, ISNULL(e.Phone, '') AS Phone, 
                               ISNULL(e.Email, '') AS Email, ISNULL(e.EmployeeCode, '') AS EmployeeCode
                        FROM Employees e
                        JOIN AuthData a ON e.Id_Employee = a.Id_Employee
                        JOIN Positions p ON e.Id_Position = p.Id_Position
                        WHERE a.Login = @login";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    Id = reader.GetInt32(0),
                                    Login = reader.GetString(1),
                                    Password = reader.GetString(2),
                                    FullName = reader.GetString(3),
                                    Position = reader.GetString(4),
                                    Phone = reader.GetString(5),
                                    Email = reader.GetString(6),
                                    EmployeeCode = reader.GetString(7)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения пользователя: {ex.Message}");
            }
            return null;
        }
        public bool RegisterUser(string login, string password, string fullName, string position, string phone, string email)
        {
            try
            {
                using (var conn = DatabaseHelper.Instance.GetOpenConnection())
                {
                    int positionId = 2;
                    string posQuery = "SELECT Id_Position FROM Positions WHERE Name = @position";
                    using (var posCmd = new SqlCommand(posQuery, conn))
                    {
                        posCmd.Parameters.AddWithValue("@position", position);
                        var result = posCmd.ExecuteScalar();
                        if (result != null)
                            positionId = Convert.ToInt32(result);
                    }
                    var nameParts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string surname = nameParts.Length > 0 ? nameParts[0] : "";
                    string name = nameParts.Length > 1 ? nameParts[1] : "";
                    string patronymic = nameParts.Length > 2 ? nameParts[2] : "";

                    string empQuery = @"
                        INSERT INTO Employees (Surname, Name, Patronymic, Phone, Email, Id_Position, EmployeeCode, HireDate)
                        VALUES (@surname, @name, @patronymic, @phone, @email, @positionId, @empCode, GETDATE());
                        SELECT SCOPE_IDENTITY();";

                    string empCode = $"EMP-{DateTime.Now:yyyyMMddHHmmss}";

                    int employeeId;
                    using (var empCmd = new SqlCommand(empQuery, conn))
                    {
                        empCmd.Parameters.AddWithValue("@surname", surname);
                        empCmd.Parameters.AddWithValue("@name", name);
                        empCmd.Parameters.AddWithValue("@patronymic", patronymic);
                        empCmd.Parameters.AddWithValue("@phone", phone ?? "");
                        empCmd.Parameters.AddWithValue("@email", email ?? "");
                        empCmd.Parameters.AddWithValue("@positionId", positionId);
                        empCmd.Parameters.AddWithValue("@empCode", empCode);

                        employeeId = Convert.ToInt32(empCmd.ExecuteScalar());
                    }
                    string authQuery = @"
                        INSERT INTO AuthData (Id_Employee, Login, Password, LastLoginDate)
                        VALUES (@empId, @login, @password, GETDATE())";

                    using (var authCmd = new SqlCommand(authQuery, conn))
                    {
                        authCmd.Parameters.AddWithValue("@empId", employeeId);
                        authCmd.Parameters.AddWithValue("@login", login);
                        authCmd.Parameters.AddWithValue("@password", password);
                        authCmd.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}");
                return false;
            }
        }
        public bool UpdateUser(string login, string fullName, string position, string phone, string email, string newPassword = null)
        {
            try
            {
                using (var conn = DatabaseHelper.Instance.GetOpenConnection())
                {
                    string getIdQuery = "SELECT Id_Employee FROM AuthData WHERE Login = @login";
                    int employeeId;
                    using (var idCmd = new SqlCommand(getIdQuery, conn))
                    {
                        idCmd.Parameters.AddWithValue("@login", login);
                        employeeId = Convert.ToInt32(idCmd.ExecuteScalar());
                    }
                    int positionId = 2;
                    string posQuery = "SELECT Id_Position FROM Positions WHERE Name = @position";
                    using (var posCmd = new SqlCommand(posQuery, conn))
                    {
                        posCmd.Parameters.AddWithValue("@position", position);
                        var result = posCmd.ExecuteScalar();
                        if (result != null)
                            positionId = Convert.ToInt32(result);
                    }

                    var nameParts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string surname = nameParts.Length > 0 ? nameParts[0] : "";
                    string name = nameParts.Length > 1 ? nameParts[1] : "";
                    string patronymic = nameParts.Length > 2 ? nameParts[2] : "";

                    string updateEmpQuery = @"
                        UPDATE Employees 
                        SET Surname = @surname, Name = @name, Patronymic = @patronymic,
                            Phone = @phone, Email = @email, Id_Position = @positionId
                        WHERE Id_Employee = @empId";

                    using (var empCmd = new SqlCommand(updateEmpQuery, conn))
                    {
                        empCmd.Parameters.AddWithValue("@surname", surname);
                        empCmd.Parameters.AddWithValue("@name", name);
                        empCmd.Parameters.AddWithValue("@patronymic", patronymic);
                        empCmd.Parameters.AddWithValue("@phone", phone ?? "");
                        empCmd.Parameters.AddWithValue("@email", email ?? "");
                        empCmd.Parameters.AddWithValue("@positionId", positionId);
                        empCmd.Parameters.AddWithValue("@empId", employeeId);
                        empCmd.ExecuteNonQuery();
                    }
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        string updatePassQuery = "UPDATE AuthData SET Password = @password WHERE Id_Employee = @empId";
                        using (var passCmd = new SqlCommand(updatePassQuery, conn))
                        {
                            passCmd.Parameters.AddWithValue("@password", newPassword);
                            passCmd.Parameters.AddWithValue("@empId", employeeId);
                            passCmd.ExecuteNonQuery();
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления пользователя: {ex.Message}");
                return false;
            }
        }
        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            try
            {
                using (var conn = DatabaseHelper.Instance.GetOpenConnection())
                {
                    string query = @"
                        SELECT e.Id_Employee, a.Login, a.Password, 
                               e.Surname + ' ' + e.Name + ' ' + ISNULL(e.Patronymic, '') AS FullName,
                               p.Name AS Position, ISNULL(e.Phone, '') AS Phone, 
                               ISNULL(e.Email, '') AS Email, ISNULL(e.EmployeeCode, '') AS EmployeeCode
                        FROM Employees e
                        JOIN AuthData a ON e.Id_Employee = a.Id_Employee
                        JOIN Positions p ON e.Id_Position = p.Id_Position
                        WHERE e.IsActive = 1
                        ORDER BY e.Surname";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetInt32(0),
                                Login = reader.GetString(1),
                                Password = reader.GetString(2),
                                FullName = reader.GetString(3),
                                Position = reader.GetString(4),
                                Phone = reader.GetString(5),
                                Email = reader.GetString(6),
                                EmployeeCode = reader.GetString(7)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения пользователей: {ex.Message}");
            }

            return users;
        }
        public List<Product> GetProducts()
        {
            var products = new List<Product>();

            try
            {
                using (var conn = DatabaseHelper.Instance.GetOpenConnection())
                {
                    string query = @"
                        SELECT p.Id_Product, p.Name, ISNULL(p.Description, '') AS Description, 
                               p.Price, c.Name AS Category, p.QuantityInStock
                        FROM Products p
                        JOIN Categories c ON p.Id_Category = c.Id_Category
                        WHERE p.IsActive = 1
                        ORDER BY c.Name, p.Name";

                    using (var cmd = new SqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                Category = reader.GetString(4),
                                QuantityInStock = reader.GetInt32(5)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения товаров: {ex.Message}");
            }

            return products;
        }

        public bool AddProduct(string name, decimal price, string category, string description = "")
        {
            try
            {
                using (var conn = DatabaseHelper.Instance.GetOpenConnection())
                {
                    int categoryId = 1;
                    string catQuery = "SELECT Id_Category FROM Categories WHERE Name = @category";
                    using (var catCmd = new SqlCommand(catQuery, conn))
                    {
                        catCmd.Parameters.AddWithValue("@category", category);
                        var result = catCmd.ExecuteScalar();
                        if (result != null)
                            categoryId = Convert.ToInt32(result);
                    }

                    string query = @"
                        INSERT INTO Products (Name, Description, Price, Id_Category, QuantityInStock, IsActive)
                        VALUES (@name, @desc, @price, @catId, 0, 1)";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@desc", description ?? "");
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@catId", categoryId);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления товара: {ex.Message}");
                return false;
            }
        }
        public bool AddProduct(string name, decimal price, string category)
        {
            return AddProduct(name, price, category, "");
        }
        public bool UpdateProduct(int id, string name, decimal price, string category, string description = "")
        {
            try
            {
                using (var conn = DatabaseHelper.Instance.GetOpenConnection())
                {
                    int categoryId = 1;
                    string catQuery = "SELECT Id_Category FROM Categories WHERE Name = @category";
                    using (var catCmd = new SqlCommand(catQuery, conn))
                    {
                        catCmd.Parameters.AddWithValue("@category", category);
                        var result = catCmd.ExecuteScalar();
                        if (result != null)
                            categoryId = Convert.ToInt32(result);
                    }

                    string query = @"
                        UPDATE Products 
                        SET Name = @name, Description = @desc, Price = @price, Id_Category = @catId
                        WHERE Id_Product = @id";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@desc", description ?? "");
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@catId", categoryId);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления товара: {ex.Message}");
                return false;
            }
        }
        public bool UpdateProduct(int id, string name, decimal price, string category)
        {
            return UpdateProduct(id, name, price, category, "");
        }
        public bool DeleteProduct(int id)
        {
            try
            {
                using (var conn = DatabaseHelper.Instance.GetOpenConnection())
                {
                    string query = "UPDATE Products SET IsActive = 0 WHERE Id_Product = @id";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления товара: {ex.Message}");
                return false;
            }
        }
        public Sale SaveSale(List<CartItem> cartItems, string paymentType)
        {
            if (cartItems == null || cartItems.Count == 0 || CurrentUserInfo == null)
            {
                MessageBox.Show("Нет товаров для сохранения или пользователь не авторизован");
                return null;
            }
            try
            {
                using (var conn = DatabaseHelper.Instance.GetOpenConnection())
                {
                    int paymentMethodId = 1;
                    string payQuery = "SELECT Id_Method FROM PaymentMethods WHERE Name LIKE @type + '%'";
                    using (var payCmd = new SqlCommand(payQuery, conn))
                    {
                        payCmd.Parameters.AddWithValue("@type", paymentType);
                        var result = payCmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            paymentMethodId = Convert.ToInt32(result);
                    }

                    decimal total = cartItems.Sum(item => item.Total);
                    DateTime saleDate = DateTime.Now;
                    string saleNumber = $"SALE-{saleDate:yyyyMMddHHmmss}";
                    string saleQuery = @"
                        INSERT INTO Sales (SaleNumber, SaleDate, Id_Employee, Id_PaymentMethod, TotalAmount, FinalAmount)
                        VALUES (@number, @saleDate, @empId, @payMethod, @total, @total);
                        SELECT SCOPE_IDENTITY();";
                    int saleId;
                    using (var saleCmd = new SqlCommand(saleQuery, conn))
                    {
                        saleCmd.Parameters.AddWithValue("@number", saleNumber);
                        saleCmd.Parameters.AddWithValue("@saleDate", saleDate);
                        saleCmd.Parameters.AddWithValue("@empId", CurrentUserInfo.Id);
                        saleCmd.Parameters.AddWithValue("@payMethod", paymentMethodId);
                        saleCmd.Parameters.AddWithValue("@total", total);
                        saleId = Convert.ToInt32(saleCmd.ExecuteScalar());
                    }

                    foreach (var item in cartItems)
                    {
                        if (item == null) continue;

                        string itemQuery = @"
                            INSERT INTO SaleItems (Id_Sale, Id_Product, Quantity, Price, Total)
                            VALUES (@saleId, @prodId, @qty, @price, @total)";

                        using (var itemCmd = new SqlCommand(itemQuery, conn))
                        {
                            itemCmd.Parameters.AddWithValue("@saleId", saleId);
                            itemCmd.Parameters.AddWithValue("@prodId", item.ProductId);
                            itemCmd.Parameters.AddWithValue("@qty", item.Quantity);
                            itemCmd.Parameters.AddWithValue("@price", item.Price);
                            itemCmd.Parameters.AddWithValue("@total", item.Total);
                            itemCmd.ExecuteNonQuery();
                        }
                    }

                    return new Sale
                    {
                        Id = saleId,
                        SaleNumber = saleNumber,
                        SaleDate = saleDate,
                        Cashier = CurrentUserInfo.FullName,
                        PaymentType = paymentType,
                        TotalAmount = total,
                        Items = cartItems
                            .Where(item => item != null)
                            .Select(item => new SaleItem
                            {
                                ProductName = item.Name,
                                Quantity = item.Quantity,
                                Price = item.Price
                            })
                            .ToList()
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения продажи: {ex.Message}");
                return null;
            }
        }
        public List<Sale> GetSales(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var sales = new List<Sale>();
            try
            {
                using (var conn = DatabaseHelper.Instance.GetOpenConnection())
                {
                    string query = @"
                        SELECT s.Id_Sale, s.SaleNumber, s.SaleDate, 
                               e.Surname + ' ' + e.Name AS Cashier,
                               s.TotalAmount, pm.Name AS PaymentType
                        FROM Sales s
                        JOIN Employees e ON s.Id_Employee = e.Id_Employee
                        JOIN PaymentMethods pm ON s.Id_PaymentMethod = pm.Id_Method
                        WHERE 1=1";

                    if (fromDate.HasValue)
                        query += " AND s.SaleDate >= @fromDate";
                    if (toDate.HasValue)
                        query += " AND s.SaleDate <= @toDate";

                    query += " ORDER BY s.SaleDate DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        if (fromDate.HasValue)
                            cmd.Parameters.AddWithValue("@fromDate", fromDate.Value);
                        if (toDate.HasValue)
                            cmd.Parameters.AddWithValue("@toDate", toDate.Value);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var sale = new Sale
                                {
                                    Id = reader.GetInt32(0),
                                    SaleNumber = reader.GetString(1),
                                    SaleDate = reader.GetDateTime(2),
                                    Cashier = reader.GetString(3),
                                    TotalAmount = reader.GetDecimal(4),
                                    PaymentType = reader.GetString(5),
                                    Items = new List<SaleItem>()
                                };
                                sales.Add(sale);
                            }
                        }
                    }
                    foreach (var sale in sales)
                    {
                        if (sale == null) continue;

                        string itemsQuery = @"
                            SELECT p.Name, si.Quantity, si.Price
                            FROM SaleItems si
                            JOIN Products p ON si.Id_Product = p.Id_Product
                            WHERE si.Id_Sale = @saleId";

                        using (var cmd = new SqlCommand(itemsQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@saleId", sale.Id);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sale.Items.Add(new SaleItem
                                    {
                                        ProductName = reader.IsDBNull(0) ? "Неизвестный товар" : reader.GetString(0),
                                        Quantity = reader.GetInt32(1),
                                        Price = reader.GetDecimal(2)
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения продаж: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Sale>();
            }

            return sales;
        }
        public List<Sale> GetUserSales(string login, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (string.IsNullOrEmpty(login))
                return new List<Sale>();

            var user = GetUserByLogin(login);
            if (user == null)
                return new List<Sale>();

            var allSales = GetSales(fromDate, toDate);
            return allSales.Where(s => s != null && s.Cashier == user.FullName).ToList();
        }
        public Dictionary<string, object> GetSalesStatistics(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var stats = new Dictionary<string, object>();
            var sales = GetSales(fromDate, toDate);

            var validSales = sales.Where(s => s != null).ToList();

            stats["TotalSales"] = validSales.Count;
            stats["TotalAmount"] = validSales.Count > 0 ? validSales.Sum(s => s.TotalAmount) : 0;
            stats["AverageSale"] = validSales.Count > 0 ? validSales.Average(s => s.TotalAmount) : 0;

            var popularPayment = validSales
                .Where(s => !string.IsNullOrEmpty(s.PaymentType))
                .GroupBy(s => s.PaymentType)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            stats["MostPopularPayment"] = popularPayment?.Key ?? "Нет данных";

            return stats;
        }
        public Dictionary<string, object> GetUserStatistics(string login, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var stats = new Dictionary<string, object>();

            if (string.IsNullOrEmpty(login))
            {
                stats["TotalSales"] = 0;
                stats["TotalAmount"] = 0m;
                stats["AverageSale"] = 0m;
                return stats;
            }

            var userSales = GetUserSales(login, fromDate, toDate);
            var validSales = userSales.Where(s => s != null).ToList();
            stats["TotalSales"] = validSales.Count;
            stats["TotalAmount"] = validSales.Count > 0 ? validSales.Sum(s => s.TotalAmount) : 0m;
            stats["AverageSale"] = validSales.Count > 0 ? validSales.Average(s => s.TotalAmount) : 0m;

            return stats;
        }
    }
}
