using DynamicData;
using DynamicData.Binding;
using System.Collections.ObjectModel;

internal class Program
{
    /*
     *
     * Dynamic Data provides two collection implementations, an observable list and an observable cache that expose changes to
     * the collection via an observable change set.
     *
     * The resulting observable change sets can be manipulated and transformed using Dynamic Data’s robust
     * and powerful array of change set operators.
     *
     * These operators receive change notifications, apply some logic, and subsequently provide their own change notifications.
     *
     * Using Dynamic Data’s collections and change set operators make in-memory data management extremely easy and can reduce the
     * size and complexity of your code base by abstracting complicated and often repetitive collection based operations.
 */

    public record Trade(long Id, bool isLive);
    public record TradeProxy(Trade trade, DateTime Timestamp);

    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        //Given a Trade object create an observable list like this
        var myTrades = new SourceList<Trade>();

        //or a cache which requires that a unique key is specified
        var myTradesCace = new SourceCache<Trade, long>(trade => trade.Id);

        //Either of these collections are made observable by calling the Connect() method which produces an observable change set.

        var myObservableTrades = myTrades.Connect();

        /*
        This example first filters a stream of trades to select only live trades,
        then creates a proxy for each live trade,
        and finally orders the results by most recent first.

        The resulting trade proxies are bound on the dispatcher thread to the specified observable collection.
        */

        ReadOnlyObservableCollection<TradeProxy> data;

        var loader = myObservableTrades
            .Filter(trade => trade.isLive) //filter on live trades only
            .Transform(trade => new TradeProxy(trade, DateTime.UtcNow))         //create a proxy
            .Sort(SortExpressionComparer<TradeProxy>.Descending(t => t.Timestamp))
            //.ObserveOnDispatcher()          //ensure operation is on the UI thread
            .Bind(out data)         //Populate the observable collection
            .DisposeMany()          //Dispose TradeProxy when no longer required
            .Subscribe();
    }
}