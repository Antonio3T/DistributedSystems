namespace _1
{
    public class Address
    {
        public string Street { get; set; }
        public string Zip { get; set; }
        public string DoorNumber { get; set; }
        public string City { get; set; }
        public string Municipality { get; set; }
        public string Ownership { get; set; }
        public string SendtBy { get; set; }

        public Address(string street, string zip, string doorNumber, string city, string municipality, string ownership, string sentBy)
        {
            Street = street;
            Zip = zip;
            DoorNumber = doorNumber;
            City = city;
            Municipality = municipality;
            Ownership = ownership;
            SendtBy = sentBy;
        }
        public override string ToString()
        {
            return $"{Street}, {Zip}, {DoorNumber}, {City}, {Municipality}, {Ownership}";
        }
    }
}
