using Prometheus;

public static class MetricsUtility
{
    private static MetricServer? _metricServer;

    public static void StartMetricsServer(int port)
    {
        _metricServer = new MetricServer(port);
        _metricServer.Start();
    }

    public static Counter CreateCounter(string name, string help)
    {
        return Metrics.CreateCounter(name, help);
    }
}
