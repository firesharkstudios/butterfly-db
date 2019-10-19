/* Any copyright is dedicated to the Public Domain.
 * http://creativecommons.org/publicdomain/zero/1.0/ */

using System.Threading.Tasks;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Butterfly.Db.Test {
    [TestClass]
    public class ParseTest {
        [TestMethod]
        public async Task Parse() {
            var database = new Memory.MemoryDatabase();
            await database.CreateFromResourceFileAsync(Assembly.GetExecutingAssembly(), "Butterfly.Db.Test.butterfly_db_test.sql");
            var tableRefs = StatementFromRef.ParseFromRefs(database, "employee_contact ec INNER JOIN employee e ON ec.employee_id=e.id AND 1=2 left JOIN department d on e.department_id=d.id and 1=2");
            Assert.AreEqual(3, tableRefs.Length);
            Assert.AreEqual(tableRefs[0].table.Name, "employee_contact");
            Assert.AreEqual(tableRefs[0].tableAlias, "ec");
            Assert.AreEqual(tableRefs[1].table.Name, "employee");
            Assert.AreEqual(tableRefs[1].tableAlias, "e");
            Assert.AreEqual(tableRefs[2].table.Name, "department");
            Assert.AreEqual(tableRefs[2].tableAlias, "d");

            var selectStatement = new SelectStatement(database, "SELECT * FROM employee_contact WHERE contact_id=@contactId ORDER BY seq");
            Assert.AreEqual("*", selectStatement.selectClause);
            Assert.AreEqual("employee_contact", selectStatement.fromClause);
            Assert.AreEqual("contact_id=@contactId", selectStatement.whereClause);
            Assert.AreEqual("seq", selectStatement.orderByClause);

            var selectStatement2 = new SelectStatement(database, @"SELECT ec.id 
                FROM employee_contact ec 
                    INNER JOIN employee e ON ec.employee_id=e.id 
                WHERE contact_id=@contactId ORDER BY seq");
            Assert.AreEqual("ec.id", selectStatement2.selectClause);

            var selectStatement3 = new SelectStatement(database, 
                @"SELECT ec.*,
                (SELECT name FROM employee e WHERE e.id=ec.employee_id) name,
                FROM employee_contact ec"
            );
            Assert.AreEqual("employee_contact ec", selectStatement3.fromClause);
        }

    }
}
