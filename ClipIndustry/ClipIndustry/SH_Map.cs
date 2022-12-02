using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ClipIndustry
{
    [Serializable]
    internal class SH_Map
    {
        public Dictionary<string, SH_MapRegion> mapRegions = new Dictionary<string, SH_MapRegion>();
        public int mapWidth = 0;
        public int mapHeight = 0;
        public SH_MapTile[,] mapTiles;

        public SH_Map() 
        {
            mapTiles = new SH_MapTile[0, 0];
        }

        static public SH_Map newMapFromText(string filePath)
        {
            SH_Map _map = new SH_Map();
            string[] fileLines = File.ReadAllLines(filePath);

            string fileText = "";
            foreach (string line in fileLines)
            {
                fileText += line;
            }

            string mapData = fileText.Substring(6);

            //3xHex - Height
            //3xHex - Width
            string mapInfo = fileText.Substring(0, 6);
            _map.mapHeight = Convert.ToInt32(mapInfo.Substring(0, 3), 16);
            _map.mapWidth = Convert.ToInt32(mapInfo.Substring(3, 3), 16);

            //Console.WriteLine(String.Format("{0}, {1}", mapWidth, mapHeight));

            _map.mapTiles = new SH_MapTile[_map.mapWidth, _map.mapHeight];

            //Console.WriteLine(String.Format("Len: {0}", mapData.Length));

            for(int i = 0; i < _map.mapTiles.Length; i++)
            {
                int x = i % _map.mapWidth;
                int y = (int)MathF.Floor(i / _map.mapWidth);

                //just in case
                bool madeNew = false;

                //Search the regions dictionary for a region. If we have one, set the map tile region to this region
                //If we dont have one, create on and set the map tile region to the new region
                SH_MapRegion _region;
                if (_map.mapRegions.TryGetValue(mapData[i].ToString() , out _region)) 
                {
                    madeNew = false;
                }
                else
                {
                    madeNew = true;
                    _region = new SH_MapRegion();
                    _region.regionIdentifier = mapData[i].ToString();
                    _map.mapRegions.Add(mapData[i].ToString(), _region);
                }

                //Debug shiz
                //Console.WriteLine(String.Format("{0}: {1}, {2} ({3}) " + (madeNew ? "NEW" : "LOAD"), i, x, y, mapData[i]));

                SH_MapTile _tile = new SH_MapTile(_region);
                _region.regionMapTiles.Add(_tile);
                _map.mapTiles[x, y] = _tile;

            }

            return _map;
        }

        public bool isValidPosition(int _x, int _y)
        {
            return _x < mapWidth && _y < mapHeight;
        }
    }

    [Serializable]
    internal class SH_MapTile
    {
        public SH_MapTile(SH_MapRegion _region)
        {
            region = _region;
        }
        public SH_MapRegion region;
    }

    [Serializable]
    internal class SH_MapRegion
    {
        public string regionIdentifier = "";
        public List<SH_MapTile> regionMapTiles = new List<SH_MapTile>();
    }
}
