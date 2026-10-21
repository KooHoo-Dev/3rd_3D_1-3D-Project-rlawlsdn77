namespace OverFixed
{
    public enum ItemType
    {
        Scrap,
        BatteryPack,
        Crystar
    }
    
    public class Item
    {
        public ItemType ItemType;
        public int ID;
        public int Reward;
    }
}