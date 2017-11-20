using System.Collections;
using System.Collections.Generic;

public class PlayerStatus
{
    public bool done;
    public int shots;
    public List<int> score;


    public PlayerStatus()
    {
        done = false;
        shots = 0;
        score = new List<int>();
    }
}
