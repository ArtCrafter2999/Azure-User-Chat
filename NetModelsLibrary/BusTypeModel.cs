namespace NetModelsLibrary
{
    public class BusTypeModel
    {
        public BusType Type { get; set; }
        public int? FromUserId { get; set; }
        public int? ToUserId { get; set; }
        public BusTypeModel(BusType type)
        {
            Type = type;
        }
        public BusTypeModel() { }
    }
}