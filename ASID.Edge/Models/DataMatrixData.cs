namespace ASID.Edge.Models
{

    //Prefix Length	Description
    //ASID1	5	Format Version
    //T	9 digits	Transaction ID
    //P	Variable	Part Number
    //K	Variable	Kanban Number
    //Q	Variable	Quantity
    //L	Variable	Location/Station
    //D	14 digits	Timestamp


    //ASID1
    //Txxxxxxxxx
    //Pxxxxxxxxxx
    //Kxxxxxx
    //Qxxx
    //Lxxxx
    //DyyyyMMddHHmmss

    //ASID1T000000001P647187100CK278001Q28LST001D20260702153025


    public class DataMatrixData
    {
        public string TransactionId { get; set; } = "";

        public string PartNo { get; set; } = "";

        public string KanbanNo { get; set; } = "";

        public int Quantity { get; set; }

        public string Model { get; set; } = "";

        public string Location { get; set; } = "";

        public DateTime Timestamp { get; set; }
    }
}


