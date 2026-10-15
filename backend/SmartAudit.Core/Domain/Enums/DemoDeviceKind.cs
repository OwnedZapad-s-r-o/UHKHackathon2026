namespace SmartAudit.Core.Domain.Enums
{
    public enum DemoDeviceKind
    {
        // Gateway z výroby — ohlašuje se na brokeru a čeká na spárování heslem z nálepky
        Gateway,

        // Čidlo (uzel IQMESH) v dosahu virtuální gatewaye — do sítě se přidá kódem z nálepky
        Node,
    }
}
