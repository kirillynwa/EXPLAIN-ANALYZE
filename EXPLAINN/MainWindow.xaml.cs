using System;
using System.Windows;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using System.IO;
using Microsoft.Win32;

namespace EXPLAINN
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MySqlConnection myConnection;
        string resultInFile = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Connect = "Database=" + TextBox1.Text + ";Data Source=localhost;User Id=root;Password=mysql"; //информация для подключения к бд
            myConnection = new MySqlConnection(Connect); //создание объекта соединения с бд
            try
            {
                myConnection.Open(); //открытие соединения с бд
                TextBoxErr.Text = "Подключение установлено!";
            }
            catch (Exception ex) //вывод сообщения об ошибке
            {
                string err = Convert.ToString(ex);
                if (err.IndexOf("Unknown database") != -1)
                {
                    TextBoxErr.Text = "Базы данных с таким именем не существует.";
                }
                else
                {
                    TextBoxErr.Text = "Подключение не установлено, т. к. конечный компьютер отверг запрос на подключение.";
                }
            }
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove(); //перемещения окна зажатием мышки
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://cloud.mail.ru/public/a6rc/283s2nrUo"); //переход к пояснительной записке
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            myConnection.Close(); //закрытие соединения
            myConnection.Dispose(); //удаление обьекта соединения
            this.Close(); //закрытие окна
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            TextBoxResultEx.Text = "";
            string sql = "EXPLAIN ANALYZE " + TextBox2.Text.ToUpper(); //запрос к бд
            string table;
            resultInFile = "Ваш запрос: \n" + TextBox2.Text + "\n\n----------------------------------";
            button6.Visibility = Visibility.Hidden; //скрытие кнопки "Сохранить результат"
            string rows;
            string textBoxRes = "";
            MySqlCommand myCommand = new MySqlCommand(sql, myConnection); //выполнения запроса к бд
            try
            {
                TextBlockResultEx.Text = "Результат работы EXPLAIN ANALYZE:";
                string result = myCommand.ExecuteScalar().ToString(); //сохранение результата запроса к бд
                int indexOfFirst = TextBox2.Text.ToUpper().IndexOf("FROM");
                table = TextBox2.Text.Substring(indexOfFirst + 5);
                int indexOfLast = table.IndexOf(" ");
                table = table.Substring(0, indexOfLast); //обрезка запроса для получения имени таблицы
                if (sql.IndexOf("WHERE") != -1) //проверка запроса для получения типа запроса
                {
                    textBoxRes = "\nТип запроса: выборка данных с условием ";
                }
                if ((sql.IndexOf("WHERE") != -1 & sql.IndexOf("MAX") != -1) | (sql.IndexOf("MAX") != -1))
                {
                    textBoxRes = "\nТип запроса: вычисление наибольшего значения";
                }
                if ((sql.IndexOf("WHERE") != -1 & sql.IndexOf("JOIN") != -1) | (sql.IndexOf("JOIN") != -1))
                {
                    textBoxRes = "\nТип запроса: выборка данных с соединением таблиц";
                }
                if ((sql.IndexOf("WHERE") != -1 & sql.IndexOf("AVG") != -1) | (sql.IndexOf("AVG") != -1))
                {
                    textBoxRes = "\nТип запроса: вычисление среднего аримфетического значения";
                }
                if ((sql.IndexOf("WHERE") != -1 & sql.IndexOf("SUM") != -1) | (sql.IndexOf("SUM") != -1))
                {
                    textBoxRes = "\nТип запроса: вычисление суммы";
                }
                if ((sql.IndexOf("WHERE") != -1 & sql.IndexOf("MIN") != -1) | (sql.IndexOf("MIN") != -1))
                {
                    textBoxRes = "\nТип запроса: вычисление наименьшего значения";
                }
                if (sql.IndexOf("UPDATE") != -1 )
                {
                    textBoxRes = "\nТип запроса: обновление данных";
                }
                if ((sql.IndexOf("HAVING") != -1 & sql.IndexOf("GROUP BY") != -1) |
                    (sql.IndexOf("HAVING") != -1 & sql.IndexOf("GROUP BY") != -1 & sql.IndexOf("WHERE") != -1))
                {
                    textBoxRes = "\nТип запроса: группировка данных с фильтрацией";
                }
                if ((sql.IndexOf("GROUP BY") != -1) | (sql.IndexOf("GROUP BY") != -1 & sql.IndexOf("WHERE") != -1))
                {
                    textBoxRes = "\nТип запроса: группировка данных";
                }
                if (sql.IndexOf("JOIN") != -1) //проверка на наличие допольнительных таблиц в запросе
                {
                    string table1;
                    int indexOfFirst1 = TextBox2.Text.ToUpper().IndexOf("JOIN"); //обрезка запроса для получения имени допольнительной таблицы
                    table1 = TextBox2.Text.Substring(indexOfFirst1 + 5);
                    int indexOfLast1 = table1.IndexOf(" ");
                    table1 = table1.Substring(0, indexOfLast1);
                    MySqlCommand myCommand2 = new MySqlCommand("SELECT COUNT(1) FROM " + table1, myConnection); //запрос на получение количества строк в таблице
                    rows = myCommand2.ExecuteScalar().ToString(); //результат запроса
                    textBoxRes += "\n\nАнализируемая таблица: " + table1 + "\nКоличество строк в таблице: " + rows;
                    MySqlCommand myCommand4 = new MySqlCommand("SHOW INDEXES FROM " + table1, myConnection); //запрос на получение информации об индексах в таблице
                    MySqlDataReader MyDataReader1;
                    MyDataReader1 = myCommand4.ExecuteReader();
                    int count1 = 0;
                    textBoxRes += "\nИнформация об индексах таблицы:\n";
                    while (MyDataReader1.Read()) //получение результата информации об индексах таблицы
                    {
                        count1++;
                        int unique1 = MyDataReader1.GetInt32(1);
                        string Key_name1 = MyDataReader1.GetString(2);
                        string Column_name1 = MyDataReader1.GetString(4);
                        string type1;
                        if (unique1 == 0) type1 = "Кластеризованный";
                        else type1 = "Некластеризованный";
                        textBoxRes += count1 + ") " + type1 + " индекс " + Key_name1 + " со столбцом " + Column_name1 + "\n";
                    }
                    MyDataReader1.Close();
                }
                MySqlCommand myCommand1 = new MySqlCommand("SELECT COUNT(1) FROM " + table, myConnection); //запрос на получение количества строк в таблице
                rows = myCommand1.ExecuteScalar().ToString(); //результат запроса
                textBoxRes += "\nАнализируемая таблица: " + table + "\nКоличество строк в таблице: " + rows;
                button6.Visibility = Visibility.Visible; //открытие кнопки "Сохранить результат"
                MySqlCommand myCommand3 = new MySqlCommand("SHOW INDEXES FROM " + table, myConnection); //запрос на получение информации об индексах в таблице
                MySqlDataReader MyDataReader;
                MyDataReader = myCommand3.ExecuteReader();
                int count = 0;
                textBoxRes += "\nИнформация об индексах таблицы:\n";
                while (MyDataReader.Read()) //получение результата информации об индексах таблицы
                { 
                    count++;
                    int unique = MyDataReader.GetInt32(1); 
                    string Key_name = MyDataReader.GetString(2);
                    string Column_name = MyDataReader.GetString(4);
                    string type;
                    if (unique == 0) type = "Кластеризованный";
                    else type = "Некластеризованный";
                    textBoxRes += count + ") " + type + " индекс " + Key_name + " со столбцом " + Column_name + "\n";
                }
                MyDataReader.Close();
                string path = @"C:\Program Files\Ampps\mysql\data\localhost-slow.log";
                string newPath = @"C:\Program Files\Ampps\mysql\data\localhost-slow-copy.log";
                FileInfo fileInf = new FileInfo(path);
                if (fileInf.Exists)
                {
                    fileInf.CopyTo(newPath, true); //копирование журнала медленных запросов для работы с ним
                }
                using (StreamReader reader = new StreamReader(newPath))
                {
                    string text = reader.ReadToEnd();
                    if (text.IndexOf(TextBox2.Text.ToUpper()) != -1) //получение информации о том, находится ли запрос в журнале медленных запросов
                    {
                        textBoxRes += "\nЗапрос попал в список медленных запросов";
                        result += "\n Используйте индексы";
                    }
                    else
                    {
                        textBoxRes += "\nЗапрос не попал в список медленных запросов";
                    }
                }
                FileInfo fileInf1 = new FileInfo(newPath);
                if (fileInf1.Exists)
                {
                    fileInf1.Delete(); //удаление копии журнала медленных запросов
                }
                TextBoxResultEx.Text = result;
                resultInFile += "\n" + textBoxRes + "\n\n----------------------------------" + "\n\n" + "Результат работы EXPLAIN ANALYZE:\n" + result;
                TextBoxResult.Text = textBoxRes; //вывод результата на экран

            }
            catch (Exception ex) //вывод сообщения об ошибке
            {
                TextBlockResultEx.Text = "";
                string result = Convert.ToString(ex);
                int indexOfFirst = result.IndexOf(':');  //обрезка сообщения об ошибке для корректного вывода
                result = result.Substring(indexOfFirst + 1);
                int indexOfLast = result.IndexOf("в MySql.Data");
                result = result.Substring(0, indexOfLast);
                if (result.IndexOf("doesn't exist") != -1)
                {
                    TextBoxResult.Text = result + "\r Элемента с таким названием не существует";
                }
                else
                {
                    TextBoxResult.Text = result;
                }

            }
        }

        private void button6_Click(object sender, RoutedEventArgs e) //сохранение результата в файл
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filename = saveFileDialog.FileName;
                File.WriteAllText(filename, resultInFile);
            }

        }
    }
}
