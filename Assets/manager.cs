using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class manager : MonoBehaviour {

    private GameObject[] spheres;
    private GameObject bottom;
    public Vector3 gravity;
    public float h;

    Dictionary<string, float> springs = new Dictionary<string, float>();
    Dictionary<string, int> neigboors = new Dictionary<string, int>();
    //public Hashtable springs = new Hashtable();
    public float gamma;
    public float PLASTICITY;
    public float K_SPRING;
    public float sigma;
    public float beta;
    public float collisionForce;
    public float K = 0.504f;
    public float Knear = 5.04f;
    public float max_spring;
    public float max_speed;

    private int frames = 0;
    private bool launch = false;

    public GameObject spawnO;
    public GameObject pression;
    public Rigidbody2D test;
    // Use this for initialization
    void Start() {
        
        bottom = GameObject.Find("BorderBot");
        test = pression.GetComponent<Rigidbody2D>();
    }

    public void movepression()
    {
        test.velocity = new Vector3(-100, 0, 0);
    }
    // Update is called once per frame
    void FixedUpdate() {
        
        if (launch == true)
       {
            logic();
       }
        
    }

    public void launchfunction()
    {
        Time.timeScale = 0.2f;
        launch = true;
        spheres = GameObject.FindGameObjectsWithTag("sphere");
        foreach (GameObject sphere in spheres)
        {
            sphere.GetComponent<Rigidbody2D>().gravityScale = 50.0f;
        }

    }

    public void spawn()
    {
        if (GameObject.Find("fluid") != null)
        {
            Destroy(GameObject.Find("fluid"));
            spheres = new GameObject[0];
        }
        launch = false;
        GameObject fluid = Instantiate(spawnO, new Vector3(1, 1, 0), Quaternion.identity);
        fluid.name = "fluid";
        
        frames = 0;
    }

    void logic()
    {
        if (frames % 10 == 0)
        {
            findneighboors();
        }
        viscosityimpulses();
        foreach (GameObject sphere in spheres)
        {
            var obj = sphere.GetComponent<properties>();
            //applygravity(sphere);
            //colision(sphere);
            obj.xprev = sphere.transform.position;
            obj.x = obj.xprev + Time.deltaTime * obj.vi;
        }
        
        if (frames % 2 == 0)
        {
            applyviscosity();
            springdisplacemen();
            doubledensity();
        }

        foreach (GameObject sphere in spheres)
        {

            var obj = sphere.GetComponent<properties>();
            obj.vi = (obj.x - obj.xprev) / Time.deltaTime;
            if (obj.vi.magnitude > max_speed)
            {
                obj.vi = Vector3.ClampMagnitude(obj.vi, max_speed);
            }

            sphere.GetComponent<Rigidbody2D>().velocity = obj.vi;

        }
        frames++;
    }

    void findneighboors()
    {
        foreach (GameObject sphere in spheres)
        {
            foreach (GameObject neighbor in spheres)
            {
                if (neighbor.name == sphere.name)
                {
                    continue;
                }
                var obj1 = sphere.GetComponent<properties>();
                var obj2 = neighbor.GetComponent<properties>();
                var distance = Vector3.Distance(obj2.x, obj1.x);
                var key = string.Concat(sphere.name, ";", neighbor.name);
                var key2 = string.Concat(neighbor.name, ";", sphere.name);
                if (distance / h < 1)
                {
                    if (!neigboors.ContainsKey(key) || !neigboors.ContainsKey(key2))
                    {
                        neigboors.Add(key, 0);
                        springs.Add(key, h);
                    }
                }
                else
                {
                    if (neigboors.ContainsKey(key) || neigboors.ContainsKey(key2))
                    {
                        neigboors.Remove(key);
                        springs.Remove(key);
                    }
                }
            }
        }
    }
    /*void applygravity(GameObject sphere)
    {   
        var obj = sphere.GetComponent<properties>();
        obj.vi = obj.vi + gravity;
        //Debug.Log(obj.xprev);
    }*/

    void applyviscosity(){
        foreach (KeyValuePair<string, int> item in neigboors)
        {

            var split = item.Key.Split(new char[] { ';' });
            var sphere = GameObject.Find(split[0]);
            var neighbor = GameObject.Find(split[1]);
            var obj1 = sphere.GetComponent<properties>();
            var obj2 = neighbor.GetComponent<properties>();
            var distance = Vector3.Distance(obj2.x, obj1.x);
            var key = string.Concat(sphere.name, ";", neighbor.name);

                var d = gamma * springs[key];

                if (distance > springs[key] + d)
                {
                    springs[key] = springs[key] + Time.deltaTime * PLASTICITY * (distance - springs[key] - d);
                }
                else if (distance < springs[key] - d)
                {
                    springs[key] = springs[key] - Time.deltaTime * PLASTICITY * (springs[key] - d - distance);
                }

                if (springs[key] > max_spring)
                {
                    springs[key] = max_spring;
                }
   
        }
    }

    void springdisplacemen()
    {
        foreach (KeyValuePair<string, float> spring in springs)
        {
        
            var split = spring.Key.Split(new char[] { ';' });
            var sphere = GameObject.Find(split[0]);
            var neighbor = GameObject.Find(split[1]);
            var obj1 = sphere.GetComponent<properties>();
            var obj2 = neighbor.GetComponent<properties>();
            var distance = Vector3.Distance(obj2.x, obj1.x);

            var D = Time.deltaTime * K_SPRING * (spring.Value - distance);
            //var D = Time.deltaTime * Time.deltaTime * K_SPRING * (1 - spring.Value / h) * (spring.Value - distance);
            var rij = obj2.x - obj1.x;
            rij.x = (rij.x * D * 0.5f) / distance;
            rij.y = (rij.y * D * 0.5f) / distance;
            //Debug.Log(D * 0.5f);
            //Debug.Log(rij.x);      
            obj1.x = obj1.x - rij;
            obj2.x = obj2.x + rij;
            
        }
    }

    void viscosityimpulses()
    {
        foreach (KeyValuePair<string, int> item in neigboors)
        {

            var split = item.Key.Split(new char[] { ';' });
            var sphere = GameObject.Find(split[0]);
            var neighbor = GameObject.Find(split[1]);
            var obj1 = sphere.GetComponent<properties>();
            var obj2 = neighbor.GetComponent<properties>();
            var distance = Vector3.Distance(obj2.x, obj1.x);

            var rij = obj2.x - obj1.x;
            rij.x = rij.x / distance;
            rij.y = rij.y / distance;
            var u = new Vector3(0.0f, 0.0f, 0.0f);
            u.x =  (obj1.vi.x - obj2.vi.x ) * rij.x;
            u.y = (obj1.vi.y - obj2.vi.y) * rij.y;
            if (u.magnitude > 0)
            {
                var I = new Vector3(0.0f, 0.0f, 0.0f);

                I.x = 0.5f * Time.deltaTime * (1 - distance / h) * (sigma * u.x + beta * u.x * u.x) * rij.x;
                I.x = 0.5f * Time.deltaTime * (1 - distance / h) * (sigma * u.y + beta * u.y * u.y) * rij.y;
                obj1.vi = obj1.vi - I;
                obj2.vi = obj2.vi + I;
            }
                
            
        }
    }

    /*void colision(GameObject sphere)
    {
        if (sphere.transform.position.y < -10.3f)
        {
            var obj = sphere.GetComponent<properties>();
            var modif =  (obj.transform.position.y - 10.3f) / collisionForce;
            obj.vi.y = obj.vi.y - modif;
            sphere.transform.position = new Vector3(sphere.transform.position.x, -10.3f,0.0f);
        }
    }*/

    void doubledensity()
    {
        foreach (GameObject sphere in spheres)
        {
            var obj1 = sphere.GetComponent<properties>();
            var rho = 0f;
            var rhonear = 0f;
            foreach (GameObject neighbor in spheres)
            {
                if (neighbor.name == sphere.name)
                {
                    continue;
                }
                var obj2 = neighbor.GetComponent<properties>();
                var distance = Vector3.Distance(obj2.x, obj1.x);
                var q = distance / h;
                if (q < 1)
                {
                    rho = rho + (1 - q) * (1 - q);
                    rhonear = rhonear + (1 - q) * (1 - q) * (1 - q);
                }           
            }

            var P = K * (rho - 10.0f);
            var Pnear = Knear * rhonear;
            var dx = new Vector3(0.0f, 0.0f, 0.0f);

            foreach (GameObject neighbor in spheres)
            {
                if (neighbor.name == sphere.name)
                {
                    continue;
                }
                var obj2 = neighbor.GetComponent<properties>();
                var distance = Vector3.Distance(obj2.x, obj1.x);
                var q = distance / h;
                if (q < 1)
                {
                    var rij = obj2.x - obj1.x;
                    rij.x = rij.x / distance;
                    rij.y = rij.y / distance;
                    var D = new Vector3(0.0f, 0.0f, 0.0f);
                    D.x = 0.5f * Time.deltaTime * Time.deltaTime * (P * (1 - q) + Pnear * (1 - q) * (1 - q)) * rij.x;
                    D.y = 0.5f * Time.deltaTime * Time.deltaTime * (P * (1 - q) + Pnear * (1 - q) * (1 - q)) * rij.y;
                    obj2.x = obj2.x + D;
                    dx = dx - D;
                }
            }
            
            obj1.x = obj1.x + dx;

        }
    }
}
