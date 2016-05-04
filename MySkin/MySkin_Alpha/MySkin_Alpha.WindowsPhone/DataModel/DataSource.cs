using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace MySkin_Alpha.Data
{

    public class DataItem
    {
        public DataItem(string uniqueName, string description, double area, double borderVariation, double assymmetryRate, double colorVariation, double blackness, double blueness, double redness, string imagePath, string safe, double risk)
        {
            Name = uniqueName;
            this.description = description;
            this.area = area;
            this.borderVariation = borderVariation;
            this.assymmetryRate = assymmetryRate;
            this.colorVariation = colorVariation;
            this.blackness = blackness;
            this.blueness = blueness;
            this.redness = redness;
            this.imagePath = imagePath;
            this.safe = safe;
            this.risk = risk;
        }

        public string Name { get; private set; }
        public string description { get; set; }
        public double area { get; private set; }
        public double borderVariation { get; private set; }
        public double assymmetryRate { get; private set; }
        public double colorVariation { get; private set; }
        public double blackness { get; private set; }
        public double blueness { get; private set; }
        public double redness { get; private set; }
        public string imagePath { get; private set; }
        public string safe { get; private set; }
        public double risk { get; private set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class DataGroup
    {
        public DataGroup(String uniqueId, String title, string ImagePath)
        {
            this.UniqueId = uniqueId;
            this.ImagePath = ImagePath;
            this.Items = new ObservableCollection<DataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<DataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }


    public sealed class DataSource
    {
        private static DataSource _sampleDataSource = new DataSource();
        private List<Nevus> listNevus = new List<Nevus>();
        public List<Nevus> ListNevus { get { return this.listNevus; } }
        public bool initialized = false;
        private string jsonDataText;

        private ObservableCollection<DataGroup> _groups = new ObservableCollection<DataGroup>();
        public ObservableCollection<DataGroup> Groups
        {
            get { return this._groups; }
        }
        

        public static async Task<IEnumerable<DataGroup>> GetGroupsAsync()
        {
            await _sampleDataSource.GetDataAsync();

            return _sampleDataSource.Groups;
        }

        public static async Task<DataGroup> GetGroupAsync(string uniqueId)
        {
            await _sampleDataSource.GetDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }
        public static async Task init()
        {
            await _sampleDataSource.GetDataAsync();
        }

        public static async Task<DataItem> GetItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.Name.Equals(uniqueId));
            //foreach (DataItem item in _sampleDataSource.Groups[0].Items)
            //{
            //    if (item.Name == uniqueId)
            //        return item;
            //}
            if (matches.Count() == 1) return matches.First();
            return null;
        }
        public static async Task AddNevusToDatabaseAsync(Nevus nevus)
        {
            _sampleDataSource.ListNevus.Add(nevus);
            await _sampleDataSource.WriteDataAsync(_sampleDataSource.Stringify(_sampleDataSource.ListNevus));

        }

        public string Stringify(List<Nevus> list)
        {
            JsonObject genJson = new JsonObject();
            JsonArray jsonGroups = new JsonArray();
            JsonObject jsonGroupMole = new JsonObject();
            jsonGroupMole["UniqueId"] = JsonValue.CreateStringValue("1");
            jsonGroupMole["Title"] = JsonValue.CreateStringValue("Mole");
            //Uri dataUri = new Uri("ms-appx:///Assets/Mole.jpg");
            jsonGroupMole["ImagePath"] = JsonValue.CreateStringValue("ms-appx:///Assets/Mole.jpg");
            JsonArray jsonNevuses = new JsonArray();
            foreach (Nevus nev in list)
            {
                JsonObject jsonObj = new JsonObject();
                jsonObj.SetNamedValue("Name", JsonValue.CreateStringValue(nev.name));
                jsonObj.SetNamedValue("description", JsonValue.CreateStringValue(nev.description));
                jsonObj.SetNamedValue("area", JsonValue.CreateNumberValue(nev.area));
                jsonObj.SetNamedValue("borderVariation", JsonValue.CreateNumberValue(nev.borderVariation));
                jsonObj.SetNamedValue("assymmetryRate", JsonValue.CreateNumberValue(nev.assymmetryRate));
                jsonObj.SetNamedValue("colorVariation", JsonValue.CreateNumberValue(nev.colorVariation));
                jsonObj.SetNamedValue("blackness", JsonValue.CreateNumberValue(nev.blackness));
                jsonObj.SetNamedValue("blueness", JsonValue.CreateNumberValue(nev.blueness));
                jsonObj.SetNamedValue("redness", JsonValue.CreateNumberValue(nev.redness));
                jsonObj.SetNamedValue("imagePath", JsonValue.CreateStringValue(nev.imagePath));
                jsonObj.SetNamedValue("safe", JsonValue.CreateStringValue(nev.safe));
                jsonObj.SetNamedValue("risk", JsonValue.CreateNumberValue(nev.risk));
                jsonNevuses.Add(jsonObj);
            }
            jsonGroupMole["data"] = jsonNevuses;
            jsonGroups.Add(jsonGroupMole);
            genJson["Groups"] = jsonGroups;

            return genJson.Stringify();
        }

        public static async void ClearJSON()
        {
            Uri dataUri = new Uri("ms-appx:///DataModel/Data.json");
            try
            {
                JsonObject genJson = new JsonObject();
                JsonArray jsonGroups = new JsonArray();
                JsonObject jsonGroupMole = new JsonObject();
                JsonArray jsonNevuses = new JsonArray();
                jsonGroupMole["UniqueId"] = JsonValue.CreateStringValue("1");
                jsonGroupMole["Title"] = JsonValue.CreateStringValue("Mole");
                jsonGroupMole["ImagePath"] = JsonValue.CreateStringValue("ms-appx:///Assets/Mole.jpg");
                jsonGroupMole["data"] = jsonNevuses;
                jsonGroups.Add(jsonGroupMole);
                genJson["Groups"] = jsonGroups;

                //StorageFile f = await ApplicationData.Current.LocalFolder.GetFileAsync("Data.json");
                StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
                await FileIO.WriteTextAsync(f, "");

            }
            catch (FileNotFoundException)
            {
                StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
                //StorageFile f = await ApplicationData.Current.LocalFolder.CreateFileAsync("Data.json");
                await FileIO.WriteTextAsync(f, "");
            }
        }

        public async Task WriteDataAsync(string data)
        {
            jsonDataText = data;
            Uri dataUri = new Uri("ms-appx:///DataModel/Data.json");
            try
            {
                //StorageFile f = await ApplicationData.Current.LocalFolder.GetFileAsync("Data.json");
                StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
                await FileIO.WriteTextAsync(f, data);
                
            }
            catch (FileNotFoundException)
            {
                //StorageFile f = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
                ////StorageFile f = await ApplicationData.Current.LocalFolder.CreateFileAsync("Data.json");
                //await FileIO.WriteTextAsync(f, data);
            }

            //await _sampleDataSource.GetDataAsync();
        }

        public static async void RemoveItem(string id)
        {
            int ct = 0;
            foreach(DataItem item in _sampleDataSource.Groups[0].Items)
            {
                if (item.Name==id)
                {
                    StorageFile f = await StorageFile.GetFileFromPathAsync(item.imagePath);
                    await f.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    _sampleDataSource.ListNevus.RemoveAt(ct);
                    _sampleDataSource.Groups[0].Items.RemoveAt(ct);
                }
                ct++;
            }
        }

        public List<Nevus> GetNevusList()
        {
            return _sampleDataSource.ListNevus;
        }

        public static Nevus GetNevusById(string id)
        {
            foreach (Nevus n in _sampleDataSource.ListNevus)
            {
                if (n.name == id)
                    return n;
            }
            return null;
        }




        private async Task GetDataAsync()
        {
            string jsonText;
            if (jsonDataText == null)
            {
                //StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("Data.json");
                Uri dataUri = new Uri("ms-appx:///DataModel/Data.json");
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
                jsonText = await FileIO.ReadTextAsync(file);
            }
            else jsonText = jsonDataText;
            {
                this.Groups.Clear();
                this.ListNevus.Clear();
            }
            try
            {
                JsonObject jsonObject = JsonObject.Parse(jsonText);
                JsonArray jsonArray = jsonObject["Groups"].GetArray();
                foreach (JsonValue groupValue in jsonArray)
                {
                    JsonObject groupObject = groupValue.GetObject();
                    DataGroup group = new DataGroup(groupObject["UniqueId"].GetString(),
                                                                groupObject["Title"].GetString(),
                                                                groupObject["ImagePath"].GetString());

                    foreach (JsonValue itemValue in groupObject["data"].GetArray())
                    {
                        JsonObject itemObject = itemValue.GetObject();
                        DataItem item = new DataItem(itemObject["Name"].GetString(),
                            itemObject["description"].GetString(),
                            itemObject["area"].GetNumber(),
                            itemObject["borderVariation"].GetNumber(),
                            itemObject["assymmetryRate"].GetNumber(),
                            itemObject["colorVariation"].GetNumber(),
                            itemObject["blackness"].GetNumber(),
                            itemObject["blueness"].GetNumber(),
                            itemObject["redness"].GetNumber(),
                            itemObject["imagePath"].GetString(),
                            itemObject["safe"].GetString(),
                            itemObject["risk"].GetNumber());
                        this.ListNevus.Add(new Nevus(item.Name, item.description, item.area, item.borderVariation,item.assymmetryRate, item.colorVariation, item.blackness, item.blueness, item.redness, item.imagePath));
                        group.Items.Add(item);
                    }
                    this.Groups.Add(group);
                    initialized = true;
                }
            }
            catch { }
        }
    }
}
