using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using BalahninLibrary;

namespace BalahninLibraryTests
{
    [TestClass]
    public class LibraryItemValidatorTests
    {

        [TestMethod]
        public void Validate_InventoryValid_NullReturned()
        {
            string inventory = "INV-100";
            string year = "1869";

            string result = LibraryItemValidator.Validate(inventory, year);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Validate_InventoryLowerCase_NullReturned()
        {
            string inventory = "inv-100";
            string year = "1869";

            string result = LibraryItemValidator.Validate(inventory, year);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Validate_InventoryWithSpaces_NullReturned()
        {
            string inventory = "  INV-100  ";
            string year = "1869";

            string result = LibraryItemValidator.Validate(inventory, year);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Validate_InventoryNoPrefix_ErrorReturned()
        {
            string inventory = "100";
            string year = "1869";

            string result = LibraryItemValidator.Validate(inventory, year);

            Assert.AreEqual("Инвентарный номер должен быть в формате: INV-000 (латинские буквы INV, дефис и цифры).", result);
        }

        [TestMethod]
        public void Validate_InventoryWrongPrefix_ErrorReturned()
        {
            string inventory = "ABC-100";
            string year = "1869";

            string result = LibraryItemValidator.Validate(inventory, year);

            Assert.AreEqual("Инвентарный номер должен быть в формате: INV-000 (латинские буквы INV, дефис и цифры).", result);
        }

        [TestMethod]
        public void Validate_InventoryLettersAfterDash_ErrorReturned()
        {
            string inventory = "INV-abc";
            string year = "1869";

            string result = LibraryItemValidator.Validate(inventory, year);

            Assert.AreEqual("Инвентарный номер должен быть в формате: INV-000 (латинские буквы INV, дефис и цифры).", result);
        }

        [TestMethod]
        public void Validate_Year1499_ErrorReturned()
        {
            string inventory = "INV-100";
            string year = "1499";

            string result = LibraryItemValidator.Validate(inventory, year);

            Assert.AreEqual("Введите корректный год публикации!", result);
        }

        [TestMethod]
        public void Validate_Year1500_NullReturned()
        {
            string inventory = "INV-100";
            string year = "1500";

            string result = LibraryItemValidator.Validate(inventory, year);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Validate_YearFuture_ErrorReturned()
        {
            string inventory = "INV-100";
            string year = (DateTime.Now.Year + 1).ToString();

            string result = LibraryItemValidator.Validate(inventory, year);

            Assert.AreEqual("Введите корректный год публикации!", result);
        }

        [TestMethod]
        public void Validate_YearText_ErrorReturned()
        {
            string inventory = "INV-100";
            string year = "abc";

            string result = LibraryItemValidator.Validate(inventory, year);

            Assert.AreEqual("Введите корректный год публикации!", result);
        }
    }
}