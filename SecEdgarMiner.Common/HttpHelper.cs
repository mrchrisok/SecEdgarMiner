using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SecEdgarMiner.Common
{
   public class HttpHelper
   {
	  public static async Task<string> GetResponseMessageAsync(HttpResponseMessage response)
	  {
		 var stream = await GetResponseStreamAsync(response);
		 var reader = new StreamReader(stream);
		 var message = reader.ReadToEnd();

		 return message;
	  }

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
