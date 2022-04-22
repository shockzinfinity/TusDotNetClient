// See https://aka.ms/new-console-template for more information
using TusDotNetClient;

Console.WriteLine("Hello, World!");

string direcoryName = "data";
string tusEndpoint = @"http://localhost:8008/files/";

if (Directory.Exists(direcoryName)) {
  Directory.Delete(direcoryName, true);
}

DirectoryInfo _directoryInfo = Directory.CreateDirectory(direcoryName);

var smallTextFile = new FileInfo(Path.Combine(_directoryInfo.FullName, "small_text_file.txt"));
File.WriteAllText(smallTextFile.FullName, Guid.NewGuid().ToString());

var largeSampleFile = new FileInfo(Path.Combine(_directoryInfo.FullName, "large_sample_file.bin"));
using (var fileStream = new FileStream(largeSampleFile.FullName, FileMode.Create, FileAccess.Write)) {
  var bytes = new byte[1024 * 1024];
  foreach (var _ in Enumerable.Range(0, 50)) {
    new Random().NextBytes(bytes);
    await fileStream.WriteAsync(bytes, 0, bytes.Length);
  }
}

var tus = new TusClient();
var response = await tus.GetServerInfo(tusEndpoint);
Console.WriteLine($"Version: {response.Version}");
response.Extensions.ToList().ForEach(x => Console.WriteLine(x));
response.SupportedVersions.ToList().ForEach(x => Console.WriteLine(x));

// metadata value -> base64 encoded
var fileUrl = await tus.CreateAsync(tusEndpoint, largeSampleFile.Length, new[] { ("filename", $"{largeSampleFile.Name}") });
var uploadOperation = tus.UploadAsync(fileUrl, largeSampleFile, chunkSize: 5D);

uploadOperation.Progressed += (transferred, total) => Console.WriteLine($"Progress: {transferred} / {total}");

await uploadOperation;

//var url = await tus.CreateAsync(tusEndpoint, largeSampleFile.Length);
//await tus.UploadAsync(url, largeSampleFile);

//var upload = new FileInfo(Path.Combine(_directoryInfo.FullName, $"{url.Split('/').Last()}"));
//Console.WriteLine($"upload exists: {upload.Exists}");
//Console.WriteLine($"upload length: {upload.Length}");

var testUrl = tusEndpoint + "a01489e340d24713a0ed03c8bf9c2cad";

var temp = await tus.DownloadAsync(testUrl);
var tempFileInfo = new FileInfo(Path.Combine(_directoryInfo.FullName, "temp3.bin"));
using(var fileStream = new FileStream(tempFileInfo.FullName, FileMode.Create, FileAccess.Write)) {
  await fileStream.WriteAsync(temp.ResponseBytes, 0, temp.ResponseBytes.Length);
}


var temp2 = await tus.CreateWithUploadAsync(tusEndpoint, largeSampleFile);


Console.ReadLine();