using Newtonsoft.Json;

namespace Datenmodelle
{
    //.NETs IPEndpoint class does a check preventing addresses to be constructed twice
    //we do need addresses twice, one time for the tcp server, another time for the tcp client
    //therefore we need a similar class
    public class IP
    {
        public string address { get; set; }
        public int port { get; set; }
        public IP(string _address, int _port)
        {
            this.address = _address;
            this.port = _port;
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
