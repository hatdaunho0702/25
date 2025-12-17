namespace WebApplication15.Models
{
    // Compatibility wrapper classes so legacy code that expects
    // `DB_SkinFood1Entities` or `DB_SkinFood_FinalEntities` compiles.
    // They inherit from the generated context `DB_SkinFood1Entities1`.
    public class DB_SkinFood1Entities : DB_SkinFood1Entities1
    {
        public DB_SkinFood1Entities() : base() { }
    }

    public class DB_SkinFood_FinalEntities : DB_SkinFood1Entities1
    {
        public DB_SkinFood_FinalEntities() : base() { }
    }
}
