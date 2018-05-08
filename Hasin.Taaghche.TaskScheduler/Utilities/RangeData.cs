using Hasin.Taaghche.Infrastructure.MotherShipModel.Box;
using Newtonsoft.Json;

namespace Hasin.Taaghche.TaskScheduler.Utilities
{
    /// <summary>
    ///     The RangeData is a class for keep pagining info of the generic object T.
    ///     For example the T is an array or not, or count of data array
    ///     and data object start or end index in main database table and etc.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="IRangeData" />
    public class RangeData<T> : IRangeData
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RangeData{T}" /> class.
        /// </summary>
        /// <param name="data">The real data.</param>
        /// <param name="start">The start index of this data in database table.</param>
        /// <param name="count">The count of data indexes if the data was an array.</param>
        /// <param name="totalLength">The total length of main data in database table.</param>
        /// <param name="order">The book order.</param>
        public RangeData(T data, int start = 0, int count = 0, int totalLength = 0,
            MsBox.BookOrder order = MsBox.BookOrder.NoOrder)
        {
            Data = data;
            Start = start;
            Count = count;
            TotalLength = totalLength;
            Order = order;
        }

        /// <summary>
        ///     Get the real data
        /// </summary>
        /// <value>The data.</value>
        public T Data { get; }

        /// <summary>
        ///     Gets the book order.
        /// </summary>
        /// <value>The order.</value>
        public MsBox.BookOrder Order { get; }

        /// <summary>
        ///     Gets the start index of this data in database table.
        /// </summary>
        /// <value>The start index.</value>
        public int Start { get; }

        /// <summary>
        ///     Gets the count of data indexes if the data was an array.
        /// </summary>
        /// <value>The count of data indexes.</value>
        public int Count { get; }

        /// <summary>
        ///     Gets the total length of main data in database table
        ///     from index 0 to end of table.
        /// </summary>
        /// <value>The total length of all data.</value>
        public int TotalLength { get; }

        /// <summary>
        ///     Get the real data mapped in object types.
        /// </summary>
        /// <value>The data object.</value>
        [JsonIgnore]
        public object DataObject => Data;
    }
}