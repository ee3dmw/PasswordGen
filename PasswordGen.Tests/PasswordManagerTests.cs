using System;
using NUnit.Framework;
using Moq;

namespace PasswordGen.Tests
{
    [TestFixture]
    public class PasswordManagerTests
    {
        private const string HardcodedSalt = "hardcoded-salt";

        [Test]
        public void GeneratingRandomPasswordTest()
        {
            var userId = "user1";
            Password passwordSavedByRepo = new Password();
            var datetimeAtStartOfTest = DateTime.UtcNow;

            var passwordRepository = new Mock<IRepository<Password>>();
            passwordRepository.Setup(u => u.Save(It.IsAny<Password>())).Callback<Password>(p => passwordSavedByRepo = p);

            var passwordGenerator = new PasswordManager(passwordRepository.Object);
            var password = passwordGenerator.GeneratePassword(userId);

            passwordRepository.Verify(u => u.Save(It.IsAny<Password>()), Times.Once);
            Assert.IsFalse(string.IsNullOrEmpty(password), "Check password is not empty");
            Assert.AreEqual(userId, passwordSavedByRepo.UserId, "Check saved password has userid");
            Assert.AreEqual(HardcodedSalt, passwordSavedByRepo.PasswordSalt, "Check saved password has salt");
            Assert.AreEqual(String.Concat(password, HardcodedSalt).GetHashCode().ToString(), passwordSavedByRepo.PasswordHash, "Check saved password has hash");
            Assert.IsTrue(passwordSavedByRepo.ExpiryTime < datetimeAtStartOfTest.AddSeconds(35) && passwordSavedByRepo.ExpiryTime > datetimeAtStartOfTest.AddSeconds(25), "Check saved password has expiry date");
        }

        [Test]
        public void CheckPassword_CorrectPassword_Test()
        {
            var userId = "user1";
            var password = "monkey";
            var passwordHash = String.Concat(password, HardcodedSalt).GetHashCode().ToString();
            var passwordFromRepo = new Password() { UserId = userId, ExpiryTime = DateTime.UtcNow.AddSeconds(30), PasswordHash = passwordHash, PasswordSalt = HardcodedSalt };

            var passwordRepository = new Mock<IRepository<Password>>();
            passwordRepository.Setup(u => u.Load(userId)).Returns(passwordFromRepo);

            var passwordGenerator = new PasswordManager(passwordRepository.Object);
            var isPasswordCorrect = passwordGenerator.CheckPassword(userId, password);

            Assert.IsTrue(isPasswordCorrect);
        }

        [Test]
        public void CheckPassword_WrongPassword_Test()
        {
            var userId = "user1";
            var password = "monkey";
            var passwordHash = String.Concat(password, HardcodedSalt).GetHashCode().ToString();
            var passwordFromRepo = new Password() { UserId = userId, ExpiryTime = DateTime.UtcNow.AddSeconds(30), PasswordHash = passwordHash, PasswordSalt = HardcodedSalt };

            var passwordRepository = new Mock<IRepository<Password>>();
            passwordRepository.Setup(u => u.Load(userId)).Returns(passwordFromRepo);

            var passwordGenerator = new PasswordManager(passwordRepository.Object);
            var isPasswordCorrect = passwordGenerator.CheckPassword(userId, "chicken");

            Assert.IsFalse(isPasswordCorrect);
        }

        [Test]
        public void CheckPassword_PasswordExpired_Test()
        {
            var userId = "user1";
            var password = "monkey";
            var expiryTimeInThePast = DateTime.UtcNow.AddSeconds(-10);
            var passwordHash = String.Concat(password, HardcodedSalt).GetHashCode().ToString();
            var passwordFromRepo = new Password() { UserId = userId, ExpiryTime = expiryTimeInThePast, PasswordHash = passwordHash, PasswordSalt = HardcodedSalt };

            var passwordRepository = new Mock<IRepository<Password>>();
            passwordRepository.Setup(u => u.Load(userId)).Returns(passwordFromRepo);

            var passwordGenerator = new PasswordManager(passwordRepository.Object);
            var isPasswordCorrect = passwordGenerator.CheckPassword(userId, password);

            Assert.IsFalse(isPasswordCorrect);
        }

        [Test]
        public void CheckPassword_PasswordHasNotBeenGenerated_Test()
        {
            var userId = "user1";

            var passwordRepository = new Mock<IRepository<Password>>();

            var passwordGenerator = new PasswordManager(passwordRepository.Object);
            var isPasswordCorrect = passwordGenerator.CheckPassword(userId, "monkey");

            Assert.IsFalse(isPasswordCorrect);
        }
    }
}
