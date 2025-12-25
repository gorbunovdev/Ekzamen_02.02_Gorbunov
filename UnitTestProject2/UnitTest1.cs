using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTestProject2
{
    [TestClass]
    public class UnitTest1
    {
        // Константы для расчетов (такие же как в основном приложении)
        private const double Tariff1BasePrice = 0.7;
        private const double Tariff2BasePrice = 0.3;
        private const double ExtraMinutePrice = 1.6;
        private const int Tariff1Limit = 200;
        private const int Tariff2Limit = 100;

        // Вспомогательный метод для расчета стоимости
        private (double totalCost, int extraMinutes) CalculateCost(int minutes, bool isTariff1)
        {
            int limit = isTariff1 ? Tariff1Limit : Tariff2Limit;
            double basePrice = isTariff1 ? Tariff1BasePrice : Tariff2BasePrice;

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

            return (totalCost, extraMinutes);
        }

        // ТЕСТ 1: Ввод больших чисел в поля ввода
        [TestMethod]
        public void Test1_LargeNumbersInput_Calculation()
        {
            // Arrange - максимально большое число для int
            int minutes = int.MaxValue; // 2147483647
            bool isTariff1 = true; // Тариф 1

            // Act - выполняем расчет
            var (totalCost, extraMinutes) = CalculateCost(minutes, isTariff1);

            // Assert - проверяем что расчет выполняется без ошибок
            Assert.IsTrue(totalCost > 0, "Стоимость должна быть положительной при больших числах");
            Assert.IsTrue(extraMinutes > 0, "Должны быть сверхлимитные минуты");

            // Проверяем конкретные значения
            int expectedExtraMinutes = int.MaxValue - Tariff1Limit; // 2147483447
            double expectedCost = (Tariff1BasePrice * Tariff1Limit) +
                                 (ExtraMinutePrice * expectedExtraMinutes);

            Assert.AreEqual(expectedExtraMinutes, extraMinutes, "Неверное количество сверхлимитных минут");
            Assert.AreEqual(expectedCost, totalCost, 0.001, "Неверная расчетная стоимость");
        }

        // ТЕСТ 2: Ввод отрицательных чисел в поля ввода
        [TestMethod]
        public void Test2_NegativeNumbersInput_Validation()
        {
            // Arrange - отрицательные числа
            int minutes = -100;
            string minutesText = "-100";
            string clientName = "Иванов Иван"; // Валидное имя для теста

            // Act - проверяем валидацию ввода
            bool canParseMinutes = int.TryParse(minutesText, out int parsedMinutes);
            bool isMinutesValid = parsedMinutes >= 0;
            bool isClientNameValid = !string.IsNullOrWhiteSpace(clientName);

            // Общая валидность ввода
            bool isInputValid = isClientNameValid && isMinutesValid && canParseMinutes;

            // Assert
            Assert.IsTrue(canParseMinutes, "Отрицательное число должно парситься");
            Assert.IsFalse(isMinutesValid, "Отрицательные минуты должны быть признаны невалидными");
            Assert.IsTrue(isClientNameValid, "Имя клиента должно быть валидным");
            Assert.IsFalse(isInputValid, "Общий ввод с отрицательными числами должен быть невалидным");
        }

        // ТЕСТ 3: Пустые поля ввода
        [TestMethod]
        public void Test3_EmptyFieldsInput_Validation()
        {
            // Arrange - пустые поля
            string clientName = "";
            string minutesText = "";
            string whitespaceName = "   ";

            // Act - проверяем валидацию
            bool isClientNameEmptyValid = !string.IsNullOrWhiteSpace(clientName);
            bool isWhitespaceNameValid = !string.IsNullOrWhiteSpace(whitespaceName);
            bool isMinutesTextEmptyValid = !string.IsNullOrWhiteSpace(minutesText);
            bool canParseEmptyMinutes = int.TryParse(minutesText, out _);

            // Общая валидность для пустого имени
            bool isInputEmptyValid = isClientNameEmptyValid && isMinutesTextEmptyValid;

            // Общая валидность для имени из пробелов
            bool isInputWhitespaceValid = isWhitespaceNameValid && isMinutesTextEmptyValid;

            // Assert
            Assert.IsFalse(isClientNameEmptyValid, "Пустое имя клиента должно быть невалидным");
            Assert.IsFalse(isWhitespaceNameValid, "Имя из пробелов должно быть невалидным");
            Assert.IsFalse(isMinutesTextEmptyValid, "Пустое поле минут должно быть невалидным");
            Assert.IsFalse(canParseEmptyMinutes, "Пустую строку нельзя преобразовать в число");
            Assert.IsFalse(isInputEmptyValid, "Ввод с пустыми полями должен быть невалидным");
            Assert.IsFalse(isInputWhitespaceValid, "Ввод с именем из пробелов должен быть невалидным");
        }
    }
}