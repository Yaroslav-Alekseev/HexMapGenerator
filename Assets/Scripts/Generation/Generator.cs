using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Generator : MonoBehaviour
{
    public CameraMover CameraController;
    public RectTransform FirstHexPos;
    public HexScaler HexContainer;
    public Transform SubContainerLeft;
    public Transform SubContainerRight;
    public DraftHexPrefab HexPrefab;

    [Header("Hexes settings")]
    public int HexCountX = 1;
    public int HexCountY = 1;

    [Header("Noize settings")]
    public int Seed = 21;
    public float NoiseScale = 25f;
    public int Octaves = 4;
    public float Persistance = 0.7f;
    public float Lacunarity = 2.5f;
    public Vector2 Offset = Vector2.one;
    public int BlendingValue = 3;

    [Header("Biomes")]
    public float WaterPercent = 0.27f;
    public float MountainsPercent = 0.3f;
    public float DesertPercent = 0.2f;
    public float RiverSpringsPercent = 0.1f;

    [Header("Sliders")]
    public SliderController SeedSlider; //seed
    public SliderController NoiseSlider; //noize scale
    public SliderController OctavesSlider;  //layers count
    public SliderController PersistanceSlider; //amplitude modifier
    public SliderController LacunaritySlider; //frequency modifier
    public SliderController BlendingSlider; //blends outer edges
    public SliderController WaterSlider; //ocean settings
    public SliderController MountainSlider; //mountains settings
    public SliderController DesertSlider; //desert settings
    public SliderController RiverSlider; //rivers springs settings

    [Header("Debug")]
    public bool DestroyMiddleHex = false;


    private DraftHexPrefab[,] _hexes = new DraftHexPrefab[0,0];
    private List<DraftHexPrefab> _waterHexes;
    private List<DraftHexPrefab> _landHexes;
    private List<DraftHexPrefab> _mountains;
    private List<DraftHexPrefab> _springs;
    private float[,] _rndPerlin;
    private float[,] _heights = new float[0,0];
    private List<float> _heightsForMedian;
    private float[,] _temperaturesRnd = new float[0, 0];
    private float[,] _humidity = new float[0, 0];

    private Vector3 _leftPartPos, _rightPartPos;
    private float _hexWidth = 40f;
    private float _waterLevel;
    private float _mountainsLevel = 700;
    private bool _isSwapping = false;
    private bool _invertSwap = false;
    private bool _mapIsGenerated = false;


    private void Awake()
    {
        SetDefaultValues();
    }

    private void Update()
    {
        UpdateSliders();

        if (_isSwapping)
            SwapMap();
    }


    private void SetDefaultValues()  ///IN PROGRESS!
    {
        _hexWidth = 160f;

        HexCountX = 40;
        HexCountY = 25;

        Offset = Vector2.one;

        Seed = 447581;
        SeedSlider.SliderObj.value = Seed;

        NoiseScale = 25f;
        NoiseSlider.SliderObj.value = NoiseScale;

        Octaves = 4;
        OctavesSlider.SliderObj.value = Octaves;

        Persistance = 0.7f;
        PersistanceSlider.SliderObj.value = Persistance;

        Lacunarity = 2.8f;
        LacunaritySlider.SliderObj.value = Lacunarity;

        BlendingValue = 3;
        BlendingSlider.SliderObj.value = BlendingValue;


        WaterPercent = 0.27f;
        WaterSlider.SliderObj.value = WaterPercent;

        MountainsPercent = 0.3f;
        MountainSlider.SliderObj.value = MountainsPercent;

        DesertPercent = 0.24f;
        DesertSlider.SliderObj.value = DesertPercent;

        RiverSpringsPercent = 0.02f;
        RiverSlider.SliderObj.value = RiverSpringsPercent;

    }

    private void UpdateSliders()
    {
        float value; //value from slider

        //generation settings
        value = SeedSlider.SliderObj.value;
        Seed = (int)value;
        SeedSlider.SliderValue.text = value.ToString();

        value = NoiseSlider.SliderObj.value;
        NoiseScale = value;
        NoiseSlider.SliderValue.text = value.ToString("0.00");

        value = OctavesSlider.SliderObj.value;
        Octaves = (int)value;
        OctavesSlider.SliderValue.text = value.ToString();

        value = PersistanceSlider.SliderObj.value;
        Persistance = value;
        PersistanceSlider.SliderValue.text = value.ToString("0.00");

        value = LacunaritySlider.SliderObj.value;
        Lacunarity = value;
        LacunaritySlider.SliderValue.text = value.ToString("0.00");

        value = BlendingSlider.SliderObj.value;
        BlendingValue = (int)value;
        BlendingSlider.SliderValue.text = value.ToString();

        //landscape objects
        value = WaterSlider.SliderObj.value;
        WaterPercent = value;
        WaterSlider.SliderValue.text = value.ToString("0.00");

        value = MountainSlider.SliderObj.value;
        MountainsPercent = value;
        MountainSlider.SliderValue.text = value.ToString("0.00");

        value = DesertSlider.SliderObj.value;
        DesertPercent = value;
        DesertSlider.SliderValue.text = value.ToString("0.00");

        value = RiverSlider.SliderObj.value;
        RiverSpringsPercent = value;
        RiverSlider.SliderValue.text = value.ToString("0.00");
    }

    public void Generate()
    {
        WipeMap();
        HexContainer.ResetScale();

        _temperaturesRnd = PerlinNoise.GenerateNoiseMap(HexCountX, HexCountY, Seed, NoiseScale/8, Octaves, Persistance, Lacunarity*8, Offset*2);
        _humidity = PerlinNoise.GenerateNoiseMap(HexCountX, HexCountY, Seed, NoiseScale / 15, Octaves, Persistance, Lacunarity * 15, Offset * 3);
        _heights = PerlinNoise.GenerateNoiseMap(HexCountX, HexCountY, Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);
        _heightsForMedian = new List<float>();

        float HexHeight = _hexWidth * Mathf.Sqrt(3)/2;
        _mountainsLevel = (1-MountainsPercent) * 1000;

        for (int i=0; i < HexCountY; i++)
            for (int j=0; j < HexCountX; j++)
            {
                var hex = Instantiate(HexPrefab, Vector3.zero, Quaternion.identity);
                hex.gameObject.name = "HEX " + i + "." + j;

                _heightsForMedian.Add(_heights[j, i] * 1000);

                hex.InfoScript.Height = _heights[j, i] * 1000;
                hex.Height.text = string.Format("{0:0}", _heights[j, i]*1000);
                if (hex.Height.text == "1000") ///
                    hex.Height.text = "999";

                float t = GenerateTemperature(j, i);
                hex.InfoScript.Temperature = t;
                hex.Temperature.text = t.ToString("0.00");

                float hum = _humidity[j, i];
                hex.InfoScript.Humidity = hum;
                hex.Humidity.text = hum.ToString("0.00");

                CreateBiome(hex);

                Vector3 pos = FirstHexPos.position;
                pos.x += j * _hexWidth + 0.5f*_hexWidth*(i%2);
                pos.y -= i * HexHeight;

                hex.transform.position = pos;

                if (j < HexCountX/2)
                    hex.transform.SetParent(SubContainerLeft);
                else
                    hex.transform.SetParent(SubContainerRight);

                hex.InfoScript.X = j;
                hex.InfoScript.Y = i;

                _hexes[j,i] = hex;
            }

        BlendEdges();
        FindNeighbors();
        GenerateOcean();
        DetectIslands();
        GenerateShelves();
        TweakMountains();
        GenerateHills();
        GenerateForest();
        //GenerateSprings();
        //GenerateRivers();


        ScaleMap();
        _mapIsGenerated = true;

    }


    public void WipeMap()
    {
        foreach (var hex in _hexes)
            if (hex != null)
                Destroy(hex.gameObject);

        _hexes = new DraftHexPrefab[HexCountX, HexCountY];
        _waterHexes = new List<DraftHexPrefab>();
        _landHexes = new List<DraftHexPrefab>();
        _mountains = new List<DraftHexPrefab>();
        _springs = new List<DraftHexPrefab>();


        _rndPerlin = PerlinNoise.GenerateNoiseMap(HexCountX * HexCountY, 1, Seed, NoiseScale, Octaves, Persistance, Lacunarity, Offset);

        HexContainer.ResetScale();
        //CameraController.ResetCamera();

        _mapIsGenerated = false;
        _invertSwap = false;
        SubContainerLeft.position = _leftPartPos;
        SubContainerRight.position = _rightPartPos;
    }

    public void StartSwapping()
    {
        if (_isSwapping || !_mapIsGenerated)
            return;

        _isSwapping = true;
    }

    private void SwapMap()
    {
        var posLeft = _leftPartPos;
        var posRight = _rightPartPos;

        float scaleFactor = Screen.width / 1920f;
        scaleFactor /= ((float)Screen.width / Screen.height) / (1920f / 1080);

        posLeft.x -= HexContainer.transform.position.x + Screen.width / 2f * scaleFactor;
        posRight.x += HexContainer.transform.position.x + Screen.width / 2f * scaleFactor;


        if (_invertSwap)
        {
            posLeft = _rightPartPos;
            posRight = _leftPartPos;
        }


        SubContainerLeft.transform.position = posLeft;
        SubContainerRight.transform.position = posRight;

        _isSwapping = false;
        _invertSwap = !_invertSwap;
    }

    private void ScaleMap()
    {
        HexContainer.ScaleHexesToScreenSize();

        var middleHex = _hexes[HexCountX / 2, HexCountY / 2];
        HexContainer.transform.position -= middleHex.transform.position;
        _leftPartPos = SubContainerLeft.position;
        _rightPartPos = SubContainerRight.position;

        if (DestroyMiddleHex)
            Destroy(middleHex.gameObject); ///debug
    }

    private void BlendEdges()
    {
        if (BlendingValue == 0)
            return;

        int edge = BlendingValue / 2;
        int j = 0;

        DraftHexPrefab[,] edgeHexes = new DraftHexPrefab[HexCountY, BlendingValue];
        float[,] edgeHeights = new float[HexCountY, BlendingValue];

        float[] avg = new float[HexCountY];
        float[,] rndMap = PerlinNoise.GenerateNoiseMap(HexCountY, BlendingValue, Seed, NoiseScale/833, Octaves, Persistance, Lacunarity, Offset);
        for (int i = 0; i < HexCountY; i++)
            for (int g = 0; g < BlendingValue; g++)
                rndMap[i, g] = Mathf.Lerp(5, 10, rndMap[i, g]); // => Random.Range(5, 10)

        for (int i = 0; i < HexCountY; i++)
            for (int k = 0; k < BlendingValue; k++)
            {
                if (k < edge)
                    j = k;
                else
                    j = HexCountX + edge - k - 1;

                edgeHexes[i,k] = _hexes[j, i];
                edgeHeights[i,k] = _heights[j, i];
                avg[i] += _heights[j, i];
            }

        for (int x = 0; x < HexCountY; x++)
            for (int y = 0; y < BlendingValue; y++)
            {
                avg[y] = avg[y] / BlendingValue;
                float height = edgeHeights[x,y];
                height -= (height - avg[y]) / rndMap[x, y];
                edgeHexes[x,y].Height.text = (height*1000).ToString("0");

                var color = edgeHexes[x, y].BGImage.color;
                color.a = LerpAlpha(height);
                edgeHexes[x, y].BGImage.color = color;
                //edgeHexes[x, y].BGImage.color = new Color(0, 0, 0, 1); ///debug
            }

    }

    private void FindNeighbors()
    {
        foreach (var hex in _hexes)
        {
            var hexInfo = hex.InfoScript;

            for (int i = -1; i <= 1; i++)
                for (int j = 0; j <= 1; j++)
                {
                    int x = hexInfo.X + j;
                    int y = hexInfo.Y + i;

                    if (hexInfo.Y % 2 == 0)
                    {
                        if (!(i == 0 && j == 1))
                            x -= 1;
                    }
                    else if (i == 0 && j == 0)
                        x -= 1;

                    if (x >= 0 && y >= 0 && x < HexCountX && y < HexCountY) //check if neighbor exists
                        hexInfo.Neighbors.Add(_hexes[x, y]);
                }
        }
    }

    private void GenerateOcean()
    {
        _heightsForMedian.Sort();

        int medianPos = Mathf.RoundToInt((_heightsForMedian.Count) * WaterPercent);
        if (medianPos > 0)
            medianPos -= 1;

        _waterLevel = _heightsForMedian[medianPos];

        foreach (var hex in _hexes)
        {
            float hexHeight = hex.InfoScript.Height;
            float temperature = hex.InfoScript.Temperature;

            if (_waterLevel > 0 && hexHeight <= _waterLevel && temperature > 0.13f) //water hex
            {
                hex.InfoScript.Biome = "water";
                hex.InfoScript.LandType = "water";
                hex.InfoScript.Humidity = 1;
                hex.BGImage.color = new Color(0, 0, 1, 1f);
                _waterHexes.Add(hex);
            }
            else //land hex
                _landHexes.Add(hex);

        }
    }

    private void DetectIslands()
    {
        foreach (var hex in _landHexes)
        {
            var hexInfo = hex.InfoScript;
            if (hexInfo.LandType != "")
                continue;


            List<HexInfo> landNeighbors = new List<HexInfo>();

            foreach (var neighbor in hexInfo.Neighbors) { //mainland
                if (neighbor.InfoScript.Biome != "water")
                {
                    landNeighbors.Add(neighbor.InfoScript);
                    if (landNeighbors.Count > 3)
                    {
                        hexInfo.LandType = "mainland";
                        break;
                    }
                }
            }

            List<HexInfo> subNeighbors = new List<HexInfo>();
            switch (landNeighbors.Count)
            {
                case 0: //1 hex island
                    hexInfo.LandType = "island";
                    break;

                case 1: //2-3 hex island
                    foreach (var neighbor in landNeighbors[0].Neighbors)
                    {
                        if (neighbor.InfoScript.Biome != "water")
                        {
                            subNeighbors.Add(neighbor.InfoScript);
                            if (subNeighbors.Count > 1)
                            {
                                hexInfo.LandType = "mainland";
                                break;
                            }
                        }
                    }

                    int subSubNeighbors = 0;
                    if (subNeighbors.Count == 0)
                        hexInfo.LandType = "island";
                    else
                        foreach (var neighbor in subNeighbors)
                        {
                            if (neighbor.Biome != "water")
                            {
                                subSubNeighbors++;
                                if (subSubNeighbors > 1)
                                {
                                    hexInfo.LandType = "mainland";
                                    break;
                                }
                            }
                        }

                        if (subSubNeighbors <= 1)
                            hexInfo.LandType = "island";

                    break;

                case 2: //3 hex island
                    foreach (var neighbor in hexInfo.Neighbors)
                    {
                        foreach (var subNaighbor in neighbor.InfoScript.Neighbors)
                        {
                            if (subNaighbor.InfoScript.Biome != "water")
                            {
                                hexInfo.LandType = "mainland";
                                break;
                            }
                        }

                        if (hexInfo.LandType == "mainland")
                            break;
                        else
                            hexInfo.LandType = "island";
                    }
                    break;

                default: //mainland
                    hexInfo.LandType = "mainland";
                    break;
            }
        }

    }

    private void GenerateShelves()
    {

        foreach (var hex in _landHexes)
        {
            var hexInfo = hex.InfoScript;

            foreach (var neighbor in hexInfo.Neighbors)
                if (neighbor.InfoScript.Biome == "water" && hexInfo.LandType == "mainland")
                {
                    neighbor.InfoScript.LandType = "shelf";
                    neighbor.BGImage.color = new Color(0, 0, 1, 0.5f);
                }
        }

    }

    private float GenerateTemperature(int xHex, int yHex)
    {
        float scale = (HexCountY-1) / 10f;
        float k = 5 * scale;
        float t = - Mathf.Pow( ((yHex - k) / k) , 2) + 1;

        float rnd = _temperaturesRnd[xHex, yHex];
        rnd = Mathf.Lerp(0.75f, 1f, rnd);
        t *= rnd;

        return t;
    }

    private void CreateBiome(DraftHexPrefab hex)
    {
        //alpha from height
        float alpha = hex.InfoScript.Height / 1000f;
        alpha = LerpAlpha(alpha);

        //color from biome
        Dictionary<string, Color> biomeColors = new Dictionary<string, Color>()
        {
            {"snow", new Color(1, 1, 1, 1)},
            {"show mountain", new Color(0.8f, 0.8f, 0.8f, 1)},
            {"mountain", new Color(0.5f, 0.5f, 0.5f, 1)},
            {"grass", new Color(0.29f, 0.76f, 0.25f, 1)},
            {"desert", new Color(1, 1, 0, 1)}
        };

        string biome = GetBiomName(hex);
        hex.InfoScript.Biome = biome;

        var color = biomeColors[biome];
        color.a = alpha;
        hex.BGImage.color = color;
    }

    private string GetBiomName(DraftHexPrefab hexObject)
    {
        var hex = hexObject.InfoScript;
        string biome = null;

        float maxDesertHumidity = 0.4f / 0.22f * DesertPercent;

        if (hex.Temperature <= 0.25f)
        {
            if (hex.Height <= _mountainsLevel)
                biome = "snow";
            else
            {
                biome = "show mountain";
                _mountains.Add(hexObject);
            }
        }

        else if (hex.Height > _mountainsLevel)
        {
            biome = "mountain";
            _mountains.Add(hexObject);
        }

        //else if (hex.Humidity <= 0.4f && hex.Temperature > 0.55f)
        else if (hex.Humidity <= maxDesertHumidity && hex.Temperature > 0.55f && DesertPercent >= 0.01f)
            biome = "desert";

        else
            biome = "grass";


        return biome;
    }

    private void TweakMountains()
    {
        if (MountainsPercent > 0.57f)
            return;


        foreach (var hex in _mountains)
        {
            var hexInfo = hex.InfoScript;
            List<DraftHexPrefab> neighborMountains = new List<DraftHexPrefab>();

            foreach (var neighbor in hexInfo.Neighbors)
                if (_mountains.Contains(neighbor) && neighbor.InfoScript.LandType != "delete mountain")
                    neighborMountains.Add(neighbor);


            if (neighborMountains.Count > 3)
            {
                //DELETE SOME MOUNTAINS
                while (neighborMountains.Count > 3)
                {
                    int index = _mountains.IndexOf(hex);
                    int rnd = Mathf.RoundToInt(Mathf.Lerp(0, neighborMountains.Count - 1, _rndPerlin[index, 0]));

                    neighborMountains[rnd].InfoScript.LandType = "delete mountain";
                    neighborMountains.Remove(neighborMountains[rnd]);
                }
            }
        }


        int i = 0;
        foreach (var hex in _mountains)
        {
            if (hex.InfoScript.LandType == "delete mountain")
            {
                float rnd = _rndPerlin[i, 0];
                float minDelta = 1f;
                float maxDelta = _mountainsLevel - _waterLevel + 0.1f;

                if (minDelta >= maxDelta) //water level is too high
                    hex.InfoScript.Height = _waterLevel - 0.1f;
                else //water level is normal
                {
                    float deltaHeight = Mathf.Lerp(1, (_mountainsLevel - _waterLevel + 0.1f), rnd);
                    hex.InfoScript.Height = _mountainsLevel - deltaHeight;
                }

                hex.Height.text = hex.InfoScript.Height.ToString("0");
                CreateBiome(hex);
                //hex.InfoScript.LandType = "";
                i++;
            }
        }

    }

    private void GenerateHills()
    {
        float hillMin = _waterLevel + (_mountainsLevel - _waterLevel) * 0.8f;
        float hillMax = _mountainsLevel;

        if (hillMin > hillMax) //water level is too high
            return;

        foreach (var hex in _landHexes)
        {
            var hexInfo = hex.InfoScript;

            if (hexInfo.Height >= hillMin && hexInfo.Height <= hillMax)
            {
                hexInfo.IsHill = true;
                hex.Hill.SetActive(true);
            }
        }
    }

    private void GenerateForest()
    {
        var forestMap = PerlinNoise.GenerateNoiseMap(HexCountX, HexCountY, Seed, NoiseScale / 10, Octaves, Persistance, Lacunarity * 20, Offset * 4);
        float forestP = 0.5f;
        float forestP2 = 0.6f;

        for (int i = 0; i < HexCountY; i += 3)
        {
            for (int j = 0; j < HexCountX; j += 3)
            {
                var hex = _hexes[j, i];
                var hexInfo = hex.InfoScript;

                if (hexInfo.Biome == "grass")
                    if (forestMap[j, i] <= forestP)
                    {
                        hexInfo.IsForest = true;
                        hex.Forest.SetActive(true);

                        foreach (var neighbor in hexInfo.Neighbors)
                        {
                            var neighborInfo = neighbor.InfoScript;
                            int x = neighborInfo.X;
                            int y = neighborInfo.Y;

                            if (neighborInfo.Biome == "grass")
                                if (forestMap[x, y] <= forestP2)
                                {
                                    neighborInfo.IsForest = true;
                                    neighbor.Forest.SetActive(true);
                                }
                        }
                    }
            }
        }
    }

    private void GenerateSprings()
    {
        if (RiverSpringsPercent <= 0.001)
            return;

        int medianPos = Mathf.RoundToInt(Mathf.Lerp(_heightsForMedian.Count-1, 0, RiverSpringsPercent));
        float minSpringsLevel = _heightsForMedian[medianPos];

        foreach (var hex in _landHexes)
        {
            var hexInfo = hex.InfoScript;

            if (hexInfo.Height >= minSpringsLevel)
            {
                bool isSpring = true;
                foreach (var neighbor in hexInfo.Neighbors)
                {
                    if (neighbor.InfoScript.IsSpring)
                    {
                        isSpring = false;
                        break;
                    }
                }

                if (isSpring)
                {
                    hexInfo.IsSpring = true;
                    hex.Spring.SetActive(true);
                    _springs.Add(hex);
                }
            }

        }
    }


    private void GenerateRivers()
    {
        foreach (var spring in _springs)
        {
            spring.DraftRiver.SetActive(true);
            float minDistance = float.MaxValue;
            DraftHexPrefab nearestWater = null;

            foreach (var hex in _waterHexes)
            {
                float distance = Vector3.Distance(spring.transform.position, hex.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestWater = hex;
                }
            }

            nearestWater.DraftRiverEnd.SetActive(true);
        }
    }


    private float LerpAlpha(float t)
    {
        float alpha = Mathf.Lerp(0.4f, 1, t);

        return alpha;
    }

}
