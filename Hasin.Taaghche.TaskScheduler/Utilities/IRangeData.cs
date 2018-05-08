using Hasin.Taaghche.Infrastructure.MotherShipModel.Box;

namespace Hasin.Taaghche.TaskScheduler.Utilities
{
    public interface IRangeData
    {
        int Start { get; }
        int Count { get; }
        int TotalLength { get; }
        object DataObject { get; }
        MsBox.BookOrder Order { get; }
    }
}