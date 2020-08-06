using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace Avanti.OrderService.Order
{
    public static class DataStoreStatements
    {
        private const string Schema = "\"order\"";
        private const string OrderTable = "\"order\"";

        public static string GetOrderById => $@"
            SELECT orderJson FROM {Schema}.{OrderTable}
            WHERE id = @Id
        ";

        public static string GetOrderByExternalId => $@"
            SELECT id FROM {Schema}.{OrderTable}
            WHERE orderjson->'ExternalIdentifiers' @> @JsonValue;
        ";

        public static string InsertOrder => $@"
            INSERT INTO {Schema}.{OrderTable} (orderJson, created, updated)
            VALUES (json_in(@OrderJson::cstring), @Now, @Now)
            RETURNING id
        ";
    }
}
