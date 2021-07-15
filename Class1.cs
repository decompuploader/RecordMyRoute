using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RecordMyRoute
{
  public class Class1 : Script
  {
    public bool ShowPath = true;
    public int RecordInterval = 24;
    public bool RecordInAir = false;
    public bool StopRecordOnNotOn4Wheels = true;
    public int CheckPointsToRemoveOnReset = 10;
    public ScriptSettings Config;
    public List<Vehicle> VehicleList = new List<Vehicle>();
    public List<Class1.VehicleLocation> VehicleLocations = new List<Class1.VehicleLocation>();
    public int RecordPoint;
    public bool GotCars;
    public bool ReplayOn;
    public float ResetTimer;
    public bool ResetReady;

    public Class1()
    {
      this.KeyDown += new KeyEventHandler(this.OnKeyDown);
      this.Tick += new EventHandler(this.onTick);
      this.Aborted += new EventHandler(this.OnShutdown);
      this.Interval = 1;
      this.LoadIniFile("scripts//RecordMyRoute.ini");
    }

    public Model RequestModel(string Name)
    {
      Model model;
      // ISSUE: explicit constructor call
      ((Model) ref model).\u002Ector(Name);
      ((Model) ref model).Request(250);
      if (((Model) ref model).IsInCdImage && ((Model) ref model).IsValid)
      {
        while (!((Model) ref model).IsLoaded)
          Script.Wait(50);
        return model;
      }
      ((Model) ref model).MarkAsNoLongerNeeded();
      return model;
    }

    public Model RequestModel(int Name)
    {
      Model model;
      // ISSUE: explicit constructor call
      ((Model) ref model).\u002Ector(Name);
      ((Model) ref model).Request(250);
      if (((Model) ref model).IsInCdImage && ((Model) ref model).IsValid)
      {
        while (!((Model) ref model).IsLoaded)
          Script.Wait(50);
        return model;
      }
      ((Model) ref model).MarkAsNoLongerNeeded();
      return model;
    }

    private float progresswidth(float percent) => 0.08f * percent;

    private float progressxcoord(float percent) => 0.9f + 0.04f * percent;

    private void drawSprite2(
      string textureDict,
      string textureName,
      float screenX,
      float screenY,
      float width,
      float height,
      int r,
      int g,
      int b,
      int alpha)
    {
      Function.Call((Hash) -2332038263791780395L, new InputArgument[2]
      {
        InputArgument.op_Implicit(textureDict),
        InputArgument.op_Implicit(0)
      });
      if (!(bool) Function.Call<bool>((Hash) 91750494399812324L, new InputArgument[1]
      {
        InputArgument.op_Implicit(textureDict)
      }))
        return;
      Function.Call((Hash) -1729472009930024816L, new InputArgument[12]
      {
        InputArgument.op_Implicit(textureDict),
        InputArgument.op_Implicit(textureName),
        InputArgument.op_Implicit(screenX),
        InputArgument.op_Implicit(screenY),
        InputArgument.op_Implicit(width),
        InputArgument.op_Implicit(height),
        InputArgument.op_Implicit(0),
        InputArgument.op_Implicit(r),
        InputArgument.op_Implicit(g),
        InputArgument.op_Implicit(b),
        InputArgument.op_Implicit(alpha),
        InputArgument.op_Implicit(0)
      });
    }

    private void drawText(string text, float x, float y, float scale, int r, int g, int b)
    {
      Function.Call((Hash) 2736978246810207435L, new InputArgument[1]
      {
        InputArgument.op_Implicit("STRING")
      });
      Function.Call((Hash) 7789129354908300458L, new InputArgument[1]
      {
        InputArgument.op_Implicit(text)
      });
      Function.Call((Hash) -4725643803099155390L, new InputArgument[4]
      {
        InputArgument.op_Implicit(r),
        InputArgument.op_Implicit(g),
        InputArgument.op_Implicit(b),
        InputArgument.op_Implicit((int) byte.MaxValue)
      });
      Function.Call((Hash) 560759698880214217L, new InputArgument[2]
      {
        InputArgument.op_Implicit(0.0f),
        InputArgument.op_Implicit(scale)
      });
      Function.Call((Hash) -3674552073055540649L, new InputArgument[3]
      {
        InputArgument.op_Implicit(x),
        InputArgument.op_Implicit(y),
        InputArgument.op_Implicit(0.1)
      });
    }

    public void LoadIniFile(string iniName)
    {
      try
      {
        this.Config = ScriptSettings.Load(iniName);
        this.ShowPath = (bool) this.Config.GetValue<bool>("SETUP", "ShowPath", (M0) (this.ShowPath ? 1 : 0));
        this.RecordInterval = (int) this.Config.GetValue<int>("SETUP", "RecordInterval", (M0) this.RecordInterval);
        this.RecordInAir = (bool) this.Config.GetValue<bool>("SETUP", "RecordInAir", (M0) (this.RecordInAir ? 1 : 0));
        this.StopRecordOnNotOn4Wheels = (bool) this.Config.GetValue<bool>("SETUP", "StopRecordOnNotOn4Wheels", (M0) (this.StopRecordOnNotOn4Wheels ? 1 : 0));
        this.CheckPointsToRemoveOnReset = (int) this.Config.GetValue<int>("SETUP", "CheckPointsToRemoveOnReset", (M0) this.CheckPointsToRemoveOnReset);
      }
      catch (Exception ex)
      {
        UI.Notify("~r~Error~w~: RecordMyRoute.ini Failed To Load.");
      }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
    }

    public void onTick(object sender, EventArgs e)
    {
      if (this.ReplayOn)
      {
        if (!Game.IsControlPressed(2, (Control) 177))
          this.ResetTimer = 0.0f;
        if (Game.IsControlPressed(2, (Control) 177))
        {
          int num = -1;
          if (this.VehicleLocations.Count > 0)
          {
            foreach (Class1.VehicleLocation vehicleLocation in this.VehicleLocations)
            {
              if (vehicleLocation.Location.Count > 0)
              {
                for (int index = 0; index < vehicleLocation.Location.Count; ++index)
                {
                  if (index + 1 < vehicleLocation.Location.Count && Entity.op_Equality((Entity) vehicleLocation.Car, (Entity) Game.Player.Character.CurrentVehicle) && vehicleLocation.Location.Count - this.CheckPointsToRemoveOnReset >= 0)
                    num = vehicleLocation.Location.Count - this.CheckPointsToRemoveOnReset;
                }
              }
            }
          }
          if (num >= 0)
          {
            this.ResetTimer += 0.025f;
            this.drawSprite2("timerbars", "all_black_bg", 0.89f, 0.97f, 0.21f, 0.04f, (int) byte.MaxValue, (int) byte.MaxValue, (int) byte.MaxValue, 130);
            this.drawSprite2("timerbars", "damagebarfill_128", 0.94f, 0.97f, 0.08f, 0.03f, 0, 147, (int) byte.MaxValue, 130);
            this.drawSprite2("timerbars", "damagebarfill_128", this.progressxcoord(this.ResetTimer), 0.97f, this.progresswidth(this.ResetTimer), 0.03f, 0, 147, (int) byte.MaxValue, (int) byte.MaxValue);
            this.drawSprite2("timerbar_lines", "LineMarker20_128", 0.94f, 0.97f, 0.08f, 0.03f, 0, 0, 0, (int) byte.MaxValue);
            this.drawSprite2("timerbar_lines", "LineMarker40_128", 0.94f, 0.97f, 0.08f, 0.03f, 0, 0, 0, (int) byte.MaxValue);
            this.drawSprite2("timerbar_lines", "LineMarker60_128", 0.94f, 0.97f, 0.08f, 0.03f, 0, 0, 0, (int) byte.MaxValue);
            this.drawSprite2("timerbar_lines", "LineMarker80_128", 0.94f, 0.97f, 0.08f, 0.03f, 0, 0, 0, (int) byte.MaxValue);
            this.drawText("Resetting (" + (this.ResetTimer * 100f).ToString() + "%)", 0.805f, 0.961f, 0.3f, (int) byte.MaxValue, (int) byte.MaxValue, (int) byte.MaxValue);
            if ((double) this.ResetTimer >= 1.0)
              this.ResetReady = true;
          }
          else
            UI.ShowSubtitle("Cannot Reset Vehicle Position Yet, Keep Driving ");
        }
      }
      if (Game.IsControlPressed(2, (Control) 21) && Game.IsControlJustPressed(2, (Control) 76))
      {
        if (!this.ReplayOn)
        {
          if (this.VehicleLocations.Count > 0)
            this.VehicleLocations.Clear();
          this.VehicleLocations.Add(new Class1.VehicleLocation(new List<Vector3>(), Game.Player.Character.CurrentVehicle));
          this.ResetReady = false;
          this.ReplayOn = true;
          UI.Notify("Started Recording Points");
        }
        else if (this.ReplayOn)
        {
          if (this.VehicleLocations.Count > 0)
            this.VehicleLocations.Clear();
          this.ResetReady = false;
          this.ReplayOn = false;
          UI.Notify("Ended Recording Points");
        }
      }
      try
      {
        if (this.ReplayOn)
        {
          if (this.VehicleLocations.Count > 0)
          {
            foreach (Class1.VehicleLocation vehicleLocation in this.VehicleLocations)
            {
              if (vehicleLocation.Location.Count > 0)
              {
                for (int index = 0; index < vehicleLocation.Location.Count; ++index)
                {
                  if (index + 1 < vehicleLocation.Location.Count)
                  {
                    Vector3 vector3_1 = vehicleLocation.Location[index];
                    Vector3 vector3_2 = vehicleLocation.Location[index + 1];
                    if (this.ShowPath)
                    {
                      Function.Call((Hash) 7742345298724472448L, new InputArgument[10]
                      {
                        InputArgument.op_Implicit((float) vector3_1.X),
                        InputArgument.op_Implicit((float) vector3_1.Y),
                        InputArgument.op_Implicit((float) vector3_1.Z),
                        InputArgument.op_Implicit((float) vector3_2.X),
                        InputArgument.op_Implicit((float) vector3_2.Y),
                        InputArgument.op_Implicit((float) vector3_2.Z),
                        InputArgument.op_Implicit(0),
                        InputArgument.op_Implicit(147),
                        InputArgument.op_Implicit((int) byte.MaxValue),
                        InputArgument.op_Implicit((int) byte.MaxValue)
                      });
                      Vector3 position = ((Entity) Game.Player.Character).Position;
                      if ((double) ((Vector3) ref position).DistanceTo(vector3_1) < 100.0)
                        World.DrawMarker((MarkerType) 28, vector3_1, Vector3.Zero, Vector3.Zero, new Vector3(0.4f, 0.4f, 0.4f), Color.FromArgb(0, 147, (int) byte.MaxValue));
                    }
                  }
                }
              }
            }
          }
        }
      }
      catch
      {
        UI.ShowSubtitle("error 1");
      }
      if (!this.ReplayOn)
        return;
      try
      {
        if (this.ResetReady)
        {
          if ((double) this.ResetTimer >= 1.0)
          {
            this.ResetReady = false;
            this.ResetTimer = 0.0f;
            UI.Notify("Respawn Back ");
            Game.FadeScreenOut(800);
            Script.Wait(2500);
            if (this.VehicleLocations.Count > 0)
            {
              foreach (Class1.VehicleLocation vehicleLocation in this.VehicleLocations)
              {
                if (vehicleLocation.Location.Count > 0)
                {
                  for (int index = 0; index < vehicleLocation.Location.Count; ++index)
                  {
                    if (index + 1 < vehicleLocation.Location.Count && Entity.op_Equality((Entity) vehicleLocation.Car, (Entity) Game.Player.Character.CurrentVehicle) && vehicleLocation.Location.Count - this.CheckPointsToRemoveOnReset >= 0)
                    {
                      ((Entity) vehicleLocation.Car).Position = vehicleLocation.Location[vehicleLocation.Location.Count - this.CheckPointsToRemoveOnReset];
                      ((Entity) vehicleLocation.Car).Heading = vehicleLocation.Heading[vehicleLocation.Location.Count - this.CheckPointsToRemoveOnReset];
                    }
                  }
                }
              }
            }
            Script.Wait(500);
            Game.FadeScreenIn(800);
          }
        }
      }
      catch
      {
        UI.ShowSubtitle("error 2");
      }
      if (this.RecordPoint != this.RecordInterval)
        ++this.RecordPoint;
      if (this.RecordPoint == this.RecordInterval)
      {
        try
        {
          this.RecordPoint = 0;
          foreach (Class1.VehicleLocation vehicleLocation in this.VehicleLocations)
          {
            if (Entity.op_Inequality((Entity) vehicleLocation.Car, (Entity) null))
            {
              if (!vehicleLocation.Car.IsOnAllWheels && !this.StopRecordOnNotOn4Wheels)
              {
                if (!((Entity) vehicleLocation.Car).IsInAir && !this.RecordInAir && (double) vehicleLocation.Car.Speed > 20.0)
                {
                  vehicleLocation.Heading.Add(((Entity) vehicleLocation.Car).Heading);
                  vehicleLocation.Location.Add(((Entity) vehicleLocation.Car).Position);
                }
                if (((Entity) vehicleLocation.Car).IsInAir && this.RecordInAir && (double) vehicleLocation.Car.Speed > 20.0)
                {
                  vehicleLocation.Heading.Add(((Entity) vehicleLocation.Car).Heading);
                  vehicleLocation.Location.Add(((Entity) vehicleLocation.Car).Position);
                }
              }
              if (vehicleLocation.Car.IsOnAllWheels && this.StopRecordOnNotOn4Wheels)
              {
                if (!((Entity) vehicleLocation.Car).IsInAir && !this.RecordInAir && (double) vehicleLocation.Car.Speed > 20.0)
                {
                  vehicleLocation.Heading.Add(((Entity) vehicleLocation.Car).Heading);
                  vehicleLocation.Location.Add(((Entity) vehicleLocation.Car).Position);
                }
                if (((Entity) vehicleLocation.Car).IsInAir && this.RecordInAir && (double) vehicleLocation.Car.Speed > 20.0)
                {
                  vehicleLocation.Heading.Add(((Entity) vehicleLocation.Car).Heading);
                  vehicleLocation.Location.Add(((Entity) vehicleLocation.Car).Position);
                }
              }
              if (!vehicleLocation.Car.IsOnAllWheels && this.StopRecordOnNotOn4Wheels)
              {
                if (!((Entity) vehicleLocation.Car).IsInAir && !this.RecordInAir && (double) vehicleLocation.Car.Speed > 20.0)
                {
                  vehicleLocation.Heading.Add(((Entity) vehicleLocation.Car).Heading);
                  vehicleLocation.Location.Add(((Entity) vehicleLocation.Car).Position);
                }
                if (((Entity) vehicleLocation.Car).IsInAir && this.RecordInAir && (double) vehicleLocation.Car.Speed > 20.0)
                {
                  vehicleLocation.Heading.Add(((Entity) vehicleLocation.Car).Heading);
                  vehicleLocation.Location.Add(((Entity) vehicleLocation.Car).Position);
                }
              }
            }
          }
        }
        catch
        {
          UI.ShowSubtitle("error 3");
        }
      }
    }

    private void DisplayHelpTextThisFrame(string text)
    {
      Function.Call((Hash) -8860350453193909743L, new InputArgument[1]
      {
        InputArgument.op_Implicit("STRING")
      });
      Function.Call((Hash) 7789129354908300458L, new InputArgument[1]
      {
        InputArgument.op_Implicit(text)
      });
      Function.Call((Hash) 2562546386151446694L, new InputArgument[4]
      {
        InputArgument.op_Implicit(0),
        InputArgument.op_Implicit(0),
        InputArgument.op_Implicit(1),
        InputArgument.op_Implicit(-1)
      });
    }

    public static string LoadDict(string dict)
    {
      while (true)
      {
        if (Function.Call<bool>((Hash) -3444786327252301684L, new InputArgument[1]
        {
          InputArgument.op_Implicit(dict)
        }) == 0)
        {
          Function.Call((Hash) -3189321952077349130L, new InputArgument[1]
          {
            InputArgument.op_Implicit(dict)
          });
          Script.Yield();
        }
        else
          break;
      }
      return dict;
    }

    public void OnShutdown(object sender, EventArgs e)
    {
      if (false)
        ;
    }

    public class VehicleLocation
    {
      public List<float> Heading = new List<float>();

      public List<Vector3> Location { get; set; }

      public Vehicle Car { get; set; }

      public VehicleLocation()
      {
      }

      public VehicleLocation(List<Vector3> L, Vehicle V)
      {
        this.Location = L;
        this.Car = V;
      }
    }
  }
}
