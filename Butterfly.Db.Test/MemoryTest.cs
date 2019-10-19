/* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/ */

using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Butterfly.Db.Memory;

namespace Butterfly.Db.Test {
    [TestClass]
    public class MemoryTest {
        [TestMethod]
        public async Task TestDatabase() {
            var database = new MemoryDatabase();
            await database.CreateFromResourceFileAsync(Assembly.GetExecutingAssembly(), "Butterfly.Db.Test.butterfly_db_test.sql");
            await DatabaseUnitTest.TestDatabase(database);
        }

        [TestMethod]
        public async Task TestDynamic() {
            var database = new MemoryDatabase();
            await database.CreateFromResourceFileAsync(Assembly.GetExecutingAssembly(), "Butterfly.Db.Test.butterfly_db_test.sql");
            await DynamicUnitTest.TestDatabase(database);
        }
    }

}
