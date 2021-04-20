using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RandomBook_Parser.Model;

namespace RandomBook_Parser {
    public partial class MainForm : Form {

        // коллекция книг
        List<Book> listBooks;
        IWebDriver driver;
        int yearMin;
        public MainForm() {
            InitializeComponent();
            listBooks = new List<Book>();

        }

       // Событие клика по кнопке - "Запустить"
       // запускает процесс парсинга книг
       private async void btnStart_Click(object sender, EventArgs e) {
            
            try {

                // минимальный год издания
                yearMin = Convert.ToInt32(nudMinYear.Value);

                // кол-во книг, которое нужно спарсить
                int amountBooks = Convert.ToInt32(countBookForParse.Value);

                // если кол-во книг больше 0, то начинаем парсить
                if (amountBooks <= 0) {
                    MessageBox.Show("Введено недопустимое кол-во книг для парсинга!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                btnStart.Enabled = false;
                btnExportToJSON.Enabled = false;

                await Task.Run(() => {
                        driver = new ChromeDriver();
                        driver.Url = @"http://readly.ru/books/i_am_lucky";

                        int count = 0, countPass = 0, countLowerThenMinYear = 0; // счетчик итераций

                        // свойства книги
                        string title, author, gener, description, imgUrl, year;
                        int id_book = 0;
                        string urlImageKnigo;
                        for (int i = 0; i < amountBooks; i++) {
                            // нажимаем кнопку генерации книги
                            driver.FindElement(By.ClassName("btn-orange-big-20")).Click();

                            if (driver.FindElement(By.ClassName("blvi__book_info")).Text.Split(new char[] { ',' }).Length < 3) {
                                ++countPass;
                                continue;
                            }
                           

                                // получаем название
                            title = driver.FindElement(By.ClassName("blvi__title")).Text;

                       

                             // получаем автора
                             author = driver.FindElement(By.ClassName("blvi__book_info")).Text.Split(new char[] { ',' })[0]?.ToString() ?? "none";

                            // получаем год выпуска
                            year = driver.FindElement(By.ClassName("blvi__book_info")).Text.Split(new char[] { ',' })[1]?.ToString() ?? "none";

                            // получаем жанр
                            gener = driver.FindElement(By.ClassName("blvi__book_info")).Text.Split(new char[] { ',' })[2]?.ToString() ?? "none";

                            // получаем описание
                            description = driver.FindElement(By.ClassName("book--desc")).Text;


                        int yearPublish;
                        int.TryParse(string.Join("", year.Where(c => char.IsDigit(c))), out yearPublish);

                        // если поле "выпуск" не содержит слово "год", значит все  
                        // плохо и нам такая запись не нужна. Поэтому пропускаем и 
                        // переходим к следующей итерации
                        if (!year.Contains("год")) { ++countPass; continue; }

                        if (yearPublish < yearMin) { ++countLowerThenMinYear; continue; }

                        // получаем ссылку на изображение
                        // imgUrl = driver.FindElement(By.ClassName("blvi__image")).FindElement(By.XPath(@".//a/img")).GetAttribute("src").ToString();
                        urlImageKnigo = ParseImg(title);
                        if (urlImageKnigo != null)
                            imgUrl = urlImageKnigo;
                        else {
                            countPass++;
                            continue;
                        }

                        // формируем и добавляем полученную книгу в коллекцию для экспорта
                        listBooks.Add(new Book(count, title, author, gener, description, year, imgUrl));

                            // добавляем полученную книгу в таблицу;
                            dataGridView1.Rows.Add(count, title, author, gener, description, imgUrl, year);

                            statusStrip1.Items[0].Text = $"Получено книг: {++count}";
                            statusStrip1.Items[1].Text = $"Осталось: {amountBooks - count}";
                            statusStrip1.Items[2].Text = $"Пропущено: {countPass+countLowerThenMinYear}";
                            statusStrip1.Items[3].Text = $"Записей в коллекции экспорта: {listBooks.Count}";
                        } // for



                    // оповещаем пользователя об успешном парсинге
                    MessageBox.Show($"Операция успешно завершена!\nВсего записей о книгах: {count}\nКол-во невошедших, из за ошибок в структуре записи: {countPass}\nКол-во невошедших, из за нарушения ограничения года выпуска:{countLowerThenMinYear}", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        statusStrip1.Items[0].ForeColor = Color.Green;
                        statusStrip1.Items[1].Text = "";
                        statusStrip1.Items[0].Text = $"Успешно завершено!";
                        btnStart.Enabled = true;
                        btnExportToJSON.Enabled = true;
                        driver.Dispose();
                    });

            } catch(Exception ex) {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } // try-catch


        } // btnStart_Click

        public string ParseImg(string title) {
            string url = "";
            
            IWebDriver dr = new ChromeDriver();
            dr.Url = @"https://knigopoisk.org/";

            dr.FindElement(By.Id("search_box")).SendKeys(title);
            dr.FindElement(By.Name("yt0")).Click();
            
            if (dr.FindElements(By.ClassName("list-book__link")).Count > 0) {
                dr.FindElements(By.ClassName("list-book__link"))[0].Click();
                url = dr.FindElement(By.ClassName("poster__img")).GetAttribute("src").ToString();
            }
            else {
                url = null;
            }
                
                
            dr.Dispose();
            return url;
        }

        // Событие клика по кнопке - "Экспорт"
        // экспортирует коллекцию книг в файл JSON
        private void btnExportToJSON_Click(object sender, EventArgs e) {

            // если коллекция для экспорта пуста, то ругаемся и выходим
            if(listBooks.Count <= 0) {
                MessageBox.Show("Коллекция для экспорта пуста.\nСначала спарсите пару книг.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            // получим выбранный файл, в который нужно экспортировать данные
            string fileName = saveFileDialog.FileName;

            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(List<Book>));
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate)) {
                jsonFormatter.WriteObject(fs, listBooks);
            } // using

        } // btnExportToJSON_Click

        // Событие клика по кнопке - "Выход"
        private void btnExit_Click(object sender, EventArgs e) {
            if(driver != null) driver.Dispose();
            this.Close();
        } // btnExit_Click

        private void aboutItem_Click(object sender, EventArgs e) {
            Form about = new AboutForm();
            about.ShowDialog();
        }
    }
}
