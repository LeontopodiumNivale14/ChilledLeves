namespace ChilledLeves.Enums
{
    public enum LeveKind
    {
        Battlecraft = 0,
        Fieldcraft = 1,
        Tradecraft = 2,
        LS_Battlecraft = 3,
        LS_Fieldcraft = 4,
        LS_Tradecraft = 5,

        TurninLeve = 10,
    }

    public enum GatheringRule
    {
        Search = 1 << 0,
        Procurance = 1 << 1,
        Search_Procurance = 1 << 2,
        Execution = 1 << 3,

        None = 1 << 10,
    }
}
