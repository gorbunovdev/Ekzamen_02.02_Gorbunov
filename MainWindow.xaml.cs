using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace _02._02_Gorbunov_13Bilet
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double Tariff1BasePrice = 0.7;
        private const double Tariff2BasePrice = 0.3;
        private const double ExtraMinutePrice = 1.6;
        private const int Tariff1Limit = 200;
        private const int Tariff2Limit = 100;

        private int receiptCounter = 1;

        public MainWindow()
        {
            InitializeComponent();
            cmbTariff.SelectedIndex = 0;
        }

        // Запрет цифр в поле ФИО
        private void TxtClientName_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Проверяем каждый символ - если это цифра, запрещаем ввод
            foreach (char c in e.Text)
            {
                if (char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        // Запрет букв в поле минут
        private void TxtMinutes_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Проверяем каждый символ - если это НЕ цифра, запрещаем ввод
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            // Валидация ввода
            if (string.IsNullOrWhiteSpace(txtClientName.Text))
            {
                MessageBox.Show("Введите ФИО клиента!");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMinutes.Text))
            {
                MessageBox.Show("Введите количество минут!");
                return;
            }

            if (!int.TryParse(txtMinutes.Text, out int minutes))
            {
                MessageBox.Show("Введите число в поле 'Количество минут'!");
                return;
            }

            if (minutes < 0)
            {
                MessageBox.Show("Количество минут не может быть отрицательным!");
                return;
            }

            // Выбор тарифа
            bool isTariff1 = cmbTariff.SelectedIndex == 0;
            int limit = isTariff1 ? Tariff1Limit : Tariff2Limit;
            double basePrice = isTariff1 ? Tariff1BasePrice : Tariff2BasePrice;

            // Расчет
            double totalCost;
            int extraMinutes = 0;

            if (minutes <= limit)
            {
                totalCost = basePrice * minutes;
            }
            else
            {
                extraMinutes = minutes - limit;
                totalCost = (basePrice * limit) + (ExtraMinutePrice * extraMinutes);
            }

            // Вывод результатов
            txtResult.Text = $"Клиент: {txtClientName.Text}\n" +
                           $"Тариф: {(isTariff1 ? "Тариф 1" : "Тариф 2")}\n" +
                           $"Использовано минут: {minutes}\n" +
                           $"Лимит тарифа: {limit} мин\n" +
                           $"Сверх лимита: {extraMinutes} мин\n" +
                           $"Стоимость: {totalCost:F2} руб.\n" +
                           $"Базовая ставка: {basePrice} руб./мин\n" +
                           $"Ставка сверх лимита: {ExtraMinutePrice} руб./мин";
        }

        private void BtnGenerateReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtResult.Text))
            {
                MessageBox.Show("Сначала выполните расчет!");
                return;
            }

            try
            {
                // Извлекаем данные из результата расчета
                string clientName = ExtractValue(txtResult.Text, "Клиент:");
                string tariffInfo = ExtractValue(txtResult.Text, "Тариф:");
                string minutesUsed = ExtractMinutes(txtResult.Text);
                string extraMinutes = ExtractExtraMinutes(txtResult.Text);
                double totalCost = ExtractTotalCost(txtResult.Text);

                // Создание диалога сохранения
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    FileName = $"Чек_{receiptCounter}_{DateTime.Now:yyyyMMdd}_{clientName.Replace(" ", "_")}.docx",
                    Filter = "Документ Word (*.docx)|*.docx|Все файлы (*.*)|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (saveDialog.ShowDialog() == true)
                {
                    // Создаем Word документ
                    CreateWordDocument(saveDialog.FileName, clientName, tariffInfo,
                                      minutesUsed, extraMinutes, totalCost, receiptCounter);

                    receiptCounter++;

                    MessageBox.Show($"Квитанция успешно сохранена в формате Word!\n\nФайл: {Path.GetFileName(saveDialog.FileName)}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании квитанции: {ex.Message}");
            }
        }

        private void CreateWordDocument(string filePath, string clientName, string tariffInfo,
                                      string minutesUsed, string extraMinutes, double totalCost, int receiptNumber)
        {
            // Создаем новый Word документ
            using (WordprocessingDocument wordDocument =
                   WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                // Добавляем главную часть документа
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = mainPart.Document.AppendChild(new Body());

                // Заголовок документа
                var titleParagraph = new Paragraph();
                var titleParagraphProperties = new ParagraphProperties();
                titleParagraphProperties.AppendChild(new Justification() { Val = JustificationValues.Center });
                titleParagraph.AppendChild(titleParagraphProperties);

                var titleRun = new Run();
                var titleRunProperties = new RunProperties();
                titleRunProperties.AppendChild(new Bold());
                titleRunProperties.AppendChild(new RunFonts() { Ascii = "Arial" });
                titleRunProperties.AppendChild(new FontSize() { Val = "24" });
                titleRun.RunProperties = titleRunProperties;
                titleRun.AppendChild(new Text("ООО \"Телеком\""));
                titleParagraph.AppendChild(titleRun);
                body.AppendChild(titleParagraph);

                // "Добро пожаловать"
                var welcomeParagraph = new Paragraph();
                var welcomeRun = new Run();
                var welcomeRunProperties = new RunProperties();
                welcomeRunProperties.AppendChild(new RunFonts() { Ascii = "Arial" });
                welcomeRunProperties.AppendChild(new FontSize() { Val = "20" });
                welcomeRun.RunProperties = welcomeRunProperties;
                welcomeRun.AppendChild(new Text("Добро пожаловать"));
                welcomeParagraph.AppendChild(welcomeRun);
                body.AppendChild(welcomeParagraph);

                // Информация о компании
                AddParagraph(body, "ККМ 00081542     #4287");
                AddParagraph(body, $"ИНН 7743013901");
                AddParagraph(body, $"ЭКЛЗ 7391835522");
                AddParagraph(body, $"Чек №{receiptNumber:D8}");
                AddParagraph(body, $"{DateTime.Now:dd.MM.yy HH:mm} СИС.");

                // Пустая строка
                body.AppendChild(new Paragraph(new Run(new Text(" "))));

                // Заголовок таблицы
                AddParagraph(body, "наименование услуги", true);

                // Основная информация
                AddParagraph(body, $"Телефонная связь {tariffInfo}\t{totalCost:F2} руб.");
                AddParagraph(body, $"Клиент:\t{clientName}");
                AddParagraph(body, $"Минуты:\t{minutesUsed}");

                if (extraMinutes != "0")
                {
                    AddParagraph(body, $"Сверх лимита:\t{extraMinutes} мин");
                }

                // Итоги
                AddParagraph(body, $"Итог\t={totalCost:F0} руб.", true);
                AddParagraph(body, $"Сдача\t=0 руб.");
                AddParagraph(body, $"Сумма итого:\t={totalCost:F0} руб.", true);

                // Разделитель
                AddParagraph(body, "************************");

                // Номер чека внизу
                var footerParagraph = new Paragraph();
                var footerParagraphProperties = new ParagraphProperties();
                footerParagraphProperties.AppendChild(new Justification() { Val = JustificationValues.Center });
                footerParagraph.AppendChild(footerParagraphProperties);

                var footerRun = new Run();
                var footerRunProperties = new RunProperties();
                footerRunProperties.AppendChild(new RunFonts() { Ascii = "Arial" });
                footerRunProperties.AppendChild(new FontSize() { Val = "20" });
                footerRun.RunProperties = footerRunProperties;
                footerRun.AppendChild(new Text($"      {receiptNumber:D8}# 059705"));
                footerParagraph.AppendChild(footerRun);
                body.AppendChild(footerParagraph);

                // Сохраняем изменения
                mainPart.Document.Save();
            }
        }

        private void AddParagraph(Body body, string text, bool isBold = false)
        {
            var paragraph = new Paragraph();
            var run = new Run();
            var runProperties = new RunProperties();

            runProperties.AppendChild(new RunFonts() { Ascii = "Arial" });
            runProperties.AppendChild(new FontSize() { Val = "20" });

            if (isBold)
                runProperties.AppendChild(new Bold());

            run.RunProperties = runProperties;
            run.AppendChild(new Text(text));
            paragraph.AppendChild(run);
            body.AppendChild(paragraph);
        }

        // Вспомогательные методы для извлечения данных
        private string ExtractValue(string text, string key)
        {
            int startIndex = text.IndexOf(key);
            if (startIndex >= 0)
            {
                startIndex += key.Length;
                int endIndex = text.IndexOf('\n', startIndex);
                if (endIndex < 0) endIndex = text.Length;
                return text.Substring(startIndex, endIndex - startIndex).Trim();
            }
            return "";
        }

        private string ExtractMinutes(string text)
        {
            string line = ExtractValue(text, "Использовано минут:");
            if (!string.IsNullOrEmpty(line))
            {
                string[] parts = line.Split(' ');
                return parts.Length > 0 ? parts[0] : "0";
            }
            return "0";
        }

        private string ExtractExtraMinutes(string text)
        {
            string line = ExtractValue(text, "Сверх лимита:");
            if (!string.IsNullOrEmpty(line))
            {
                string[] parts = line.Split(' ');
                return parts.Length > 0 ? parts[0] : "0";
            }
            return "0";
        }

        private double ExtractTotalCost(string text)
        {
            string line = ExtractValue(text, "Стоимость:");
            if (!string.IsNullOrEmpty(line))
            {
                line = line.Replace("руб.", "").Trim();
                if (double.TryParse(line, out double result))
                {
                    return result;
                }
            }
            return 0;
        }
    }
}