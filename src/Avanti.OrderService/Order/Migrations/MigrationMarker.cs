using Avanti.Core.RelationalData;

namespace Avanti.OrderService.Order.Migrations;

public class MigrationMarker : IMigrationMarker
{
    public string Schema => "order";
}
