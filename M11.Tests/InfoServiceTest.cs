using System;
using System.Linq;
using System.Threading.Tasks;
using M11.Common.Enums;
using M11.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace M11.Tests
{
    [TestClass]
    public class InfoServiceTest
    {
        private static readonly string Login = "*****";
        private static readonly string Password = "*****";

        [TestMethod]
        public async Task TestGetInfo()
        {
            var infoService = new InfoService();
            var info = await infoService.GetInfo(Login, Password);
            await infoService.GetAccountInfo(info.Links.FirstOrDefault(x => x.Type == LinkType.Account)?.RelativeUrl, 
                info.CookieContainer, 
                DateTime.Now, 
                DateTime.Now.AddMonths(-5));
            Assert.IsFalse(string.IsNullOrWhiteSpace(info.ContractNumber));
        }
    }
}
