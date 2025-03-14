using System;
using System.Collections.Generic;
using System.Threading;
using NetFwTypeLib;

//Will Tilstra 2025
namespace AutoFw
{
    public enum Protocol //idk it broke without the enum
    {
        TCP,
        UDP,
        ICMP
    }

    public class FirewallRule
    {
        public Protocol protocol { get; set; }
        public int? port { get; set; } // int? to allow ICMP rules to set the int to null
        public bool inbound { get; set; }

        // TCP and UDP rules
        public FirewallRule(Protocol iprotocol, int iport, bool iinbound)
        {
            protocol = iprotocol;
            port = iport;
            inbound = iinbound;
        }

        // ICMP rules
        public FirewallRule(Protocol iprotocol, bool iinbound)
        {
            protocol = iprotocol;
            inbound = iinbound;
        }

        public void DisplayRule()
        {
            string portInfo = (port.HasValue) ? "Port: " + port.Value.ToString() : "ICMP";
            string protocolInfo = protocol.ToString();
            string inboundInfo = inbound ? "Inbound" : "Outbound";

            Console.WriteLine(portInfo);
            Console.WriteLine(protocolInfo);
            Console.WriteLine(inboundInfo);
        }
    }

    class Program
    {
        static void Main()
        {
            List<FirewallRule> firewallRules = new List<FirewallRule>(); //make rule list just a list doesnt apply the rules
            bool continueInput = true;

            while (continueInput) 
            //this is my current way of getting the rules into the list this will probably be changed latter bc its slow
            {
                Console.WriteLine("Enter the protocol: ");
                string inprotocol = Console.ReadLine();

                Protocol protocol;

                if (!Enum.TryParse(inprotocol, true, out protocol))
                {
                    Console.WriteLine("Error: Please input tcp, udp, or icmp");
                    break;
                }

                int? port = null;

                if (protocol != Protocol.ICMP)
                {
                    Console.WriteLine("Enter Port Number: ");
                    port = int.Parse(Console.ReadLine());
                }
                Console.WriteLine("Is this rule inbound (y/n)?");
                string inboundInput = Console.ReadLine();
                bool isInbound = inboundInput.Equals("y", StringComparison.OrdinalIgnoreCase);

                if (protocol == Protocol.ICMP) // Add the firewall rule to the list
                {
                    firewallRules.Add(new FirewallRule(protocol, isInbound));
                }
                else
                {
                    firewallRules.Add(new FirewallRule(protocol, port.Value, isInbound));
                }

                Console.WriteLine("Add another rule (y/n)? ");
                string continueInputString = Console.ReadLine();
                if (continueInputString.Equals("n", StringComparison.OrdinalIgnoreCase))
                {
                    continueInput = false;
                }

            }
            Console.WriteLine("All Firewall Rules:");
            foreach (var rule in firewallRules)
            {
                rule.DisplayRule();
            }

            Console.WriteLine("Adding Firewall Rules...");
            foreach (var rule in firewallRules)
            {
                string ruleName = rule.port.HasValue ? rule.port.Value.ToString() : "ICMP";
                AddFirewallRule(ruleName, rule.protocol, rule.port, rule.inbound);
            }

            Console.WriteLine("All Firewall Rules have been added.");
        }

        static void AddFirewallRule(string ruleName, Protocol protocol, int? port, bool inbound) //where the magic happens
        {
            
                Type netFwPolicy2Type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(netFwPolicy2Type);

                Type netFwRuleType = Type.GetTypeFromProgID("HNetCfg.FwRule");
                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(netFwRuleType);

                firewallRule.Name = ruleName;
                firewallRule.Protocol = protocol switch
                {
                   Protocol.TCP => 6,  
                    Protocol.UDP => 17,
                    Protocol.ICMP => 1, 
                    _ => throw new ArgumentException("Invalid Protocol")
                };

                if (port.HasValue && (protocol != Protocol.ICMP))
                {
                    firewallRule.LocalPorts = port.Value.ToString();
                }


                firewallRule.Direction = inbound ? NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN : NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                firewallRule.Enabled = true;

                firewallPolicy.Rules.Add(firewallRule);
                Console.WriteLine("Firewall rule " + ruleName + "added successfully.");
            

        }
    }
}
