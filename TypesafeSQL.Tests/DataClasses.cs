using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypesafeSQL.Tests
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public int LoginAttempts { get; set; }
        public bool Disabled { get; set; }
    }

    public class UserRoleLink
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
    }

}
