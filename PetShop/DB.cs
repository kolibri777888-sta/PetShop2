using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace PetShop
{
    class DB
    {
        public static MySqlConnection Get()
        {
            string cs =
                "server=localhost;" +
                "database=petshop_db;" +
                "user=root;" +
                "pwd=root;";

            return new MySqlConnection(cs);
        }

        internal static bool TestConnection()
        {
            throw new NotImplementedException();
        }
    }
}