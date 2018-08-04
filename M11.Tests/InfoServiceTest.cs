using System.Threading.Tasks;
using M11.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace M11.Tests
{
    [TestClass]
    public class InfoServiceTest
    {
        private static readonly string Login = "****";
        private static readonly string Password = "****";

        [TestMethod]
        public async Task TestGetInfo()
        {
            var info = await new InfoService().GetInfo(Login, Password);
            Assert.IsFalse(string.IsNullOrWhiteSpace(info.ContractNumber));
        }
    }
}
