using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

namespace Beehive._1_Web_API1.Controllers
{
    [Route("[controller]")]
    public class BeehiveController : Controller
    {
        // Thresholds for each bee type
        public static Dictionary<string, int> Thresholds = new Dictionary<string, int>()
        {
            { "worker", 70 },
            { "drone", 50 },
            { "queen", 20 },
        };

        // Initialize / Reset beehive
        public static List<Bee> Init()
        {
            return Enumerable.Range(0, 30).Select((x, i) => {
                return new Bee { Type = Thresholds.ElementAt((int)Math.Floor((double)i / 10)).Key };
            }).ToList();
        }

        // List of bees
        public static List<Bee> Beehive = Init();

        // Get a Reset beehive
        [HttpGet("GetReset")]
        public JsonResult GetReset()
        {
            Beehive = Init();
            return GetBeehive();
        }

        // Get list of bees
        [HttpGet("GetBeehive")]
        public JsonResult GetBeehive()
        {
            return Json(Beehive);
        }

        // Damage all bees
        [HttpGet("GetDamageAll")]
        public JsonResult GetDamageAll()
        {
            foreach(Bee bee in Beehive)
            {
                bee.Damage(new Random().Next(0, 80));
            }
            return GetBeehive();
        }

        // Damage a singular bee
        [HttpPost("PostDamageOne")]
        public JsonResult PostDamageOne([FromBody] DamageUpdateModel vm)
        {
            Bee bee = Beehive[vm.Index];
            bee.Damage(vm.Damage);
            return GetBeehive();
        }
    }

    public class Bee
    {
        public string Type { get; set; }
        public float Health { get; private set; } = 100;
        public bool Dead { get; private set; } = false;
        public void Damage(int damage)
        {
            // Stop... they're already dead.
            if (this.Dead) return;
            // Take damage as a percentage of their current health 
            this.Health = this.Health - (this.Health / 100) * damage;
            // Check if they have reached the damage threshold
            if (this.Health < BeehiveController.Thresholds[this.Type]) this.Dead = true;
        }

    }

    public class DamageUpdateModel
    {
        public int Damage { get; set; }
        public int Index { get; set; }
    }
}