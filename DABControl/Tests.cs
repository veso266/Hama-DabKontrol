using System;


namespace DABControl
{
    class Tests
    {
        static void Main(string[] args)
        {
            int PIN = 1234; //Our PIN
            HamaParse hama = new HamaParse("192.168.88.11", PIN);

            //Console.WriteLine("Number of Stations: " + hama.GetStationList().Count);
            //Console.WriteLine("Signal: {0} \nStatus: {1} \nStation Name: {2} \nStation DLS: {3}", hama.GetSignal(),hama.GetStatus(), hama.GetStationName(), hama.GetStationDLS());


            //Console.WriteLine(hama.TuneUP());
            //Console.WriteLine(hama.TuneDOWN());
            //hama.Tune(1); //To Tune to specific service
            //Console.WriteLine("Stations:\n{0}\n", string.Join("\n", hama.GetStationList_IDs()));

            //hama.Tune(3);

            Console.WriteLine("Signal: " + hama.GetSignal());
            Console.WriteLine("Station Name: " + hama.GetStationName());
            Console.WriteLine("Station DLS: " + hama.GetStationDLS());

            
            Console.ReadLine();
        }
    }
}
