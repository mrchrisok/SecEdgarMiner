using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SecEdgarMiner.Common
{
    public class ConfigHelper
    {
        public static async Task<Stream> GetResponseStreamAsync(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync();

            // handle a gzipped response
            if (response.Content.Headers?.ContentEncoding?.FirstOrDefault() == "gzip")
                stream = new GZipStream(stream, CompressionMode.Decompress);

            // handle a deflated response
            else if (response.Content.Headers?.ContentEncoding?.FirstOrDefault() == "deflate")
                stream = new DeflateStream(stream, CompressionMode.Decompress);

            return stream;
        }
    }
}
