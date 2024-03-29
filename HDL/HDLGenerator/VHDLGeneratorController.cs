﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDL.Compiler;

namespace HDL.HDLGenerator
{
    class VHDLGeneratorController
    {
        public void GenerateCode(CompileResult data, Action<string> onComplete)
        {
            var gates = data.Gates;
            var inputs = data.Inputs;
            var outputs = data.Outputs;

            var signals = new List<Signal>();
            foreach (var gate in gates)
            {
                signals.AddRange(gate.Signals);
            }

            signals = signals.Distinct().ToList();
            signals = signals.Where(x => !inputs.Contains(x)).ToList();
            signals.ForEach(x => x.Name = "s_" + x.Name);

            var result =
                $"--------------------------------------------------\n" +
                $"-- Code generated by Grzegorz Sabiniok code generator\n" +
                $"-- Date: {DateTime.Now.ToString("HH:mm dd/MM/yyyy")}\n" +
                $"--------------------------------------------------\n\n" +

                GenerateStandardGate("AND") +
                GenerateStandardGate("OR") +
                GenerateStandardGate("XOR") +
                GenerateStandardGate("NOT") +

                $"library ieee;\n" +
                $"use ieee.std_logic_1164.all;\n" +
                $"entity MAIN is\n" +
                $"    port(\n" +

                $"        {String.Join(" : in std_logic;\n        ", inputs.Select(x => x.Name).ToArray())} : in std_logic;\n" +
                $"        {String.Join(" : out std_logic;\n        ", outputs.Select(x => x.Name.Remove(0, 2)).ToArray())} : out std_logic\n" +
                $"    );\n" +
                $"end MAIN;\n" +
                $"\n" +
                $"architecture MAIN_BEHAVIOUR of MAIN is\n\n" +

                GenerateStandardComponent("AND") +
                GenerateStandardComponent("OR") +
                GenerateStandardComponent("XOR") +
                GenerateStandardComponent("NOT") +

                $"    signal {String.Join(", ", signals.Select(x => x.Name).ToArray())} : std_logic;\n\n" +
                $"begin\n\n" +

                String.Join("\n", gates.Select(x => GenerateInstance(x)).ToArray()) +
                $"\n" +
                $"    --outputs--\n" +
                $"\n" +
                $"    {String.Join("\n    ", outputs.Select(x => $"{x.Name.Remove(0, 2)} <= {x.Name};").ToArray())}\n" +
                $"\n" +
                $"end MAIN_BEHAVIOUR;\n";

            onComplete?.Invoke(result);

        }

        private string GenerateStandardGate(string gate)
        {
            if (gate == "NOT")
            {
                return
                    $"library ieee;\n" +
                    $"use ieee.std_logic_1164.all;\n\n" +
                    $"entity {gate}_GATE is port(a: in std_logic; y: out std_logic);\n" +
                    $"end {gate}_GATE;\n" +
                    $"\n" +
                    $"architecture {gate}_GATE_BEHAVIOUR of {gate}_GATE is\n" +
                    $"    begin\n" +
                    $"        process(a)\n" +
                    $"    begin\n" +
                    $"        y <= not a;\n" +
                    $"    end process;\n" +
                    $"end {gate}_GATE_BEHAVIOUR;\n\n";
            }
            return
                $"library ieee;\n" +
                $"use ieee.std_logic_1164.all;\n\n" +
                $"entity {gate}_GATE is port(a: in std_logic; b: in std_logic; y: out std_logic);\n" +
                $"end {gate}_GATE;\n" +
                $"\n" +
                $"architecture {gate}_GATE_BEHAVIOUR of {gate}_GATE is\n" +
                $"    begin\n" +
                $"        process(a, b)\n" +
                $"    begin\n" +
                $"       y <= a {gate} b;\n" +
                $"    end process;\n" +
                $"end {gate}_GATE_BEHAVIOUR;\n\n";
        }

        private string GenerateStandardComponent(string gate)
        {
            return
                $"component {gate}_GATE is port({String.Join(":std_logic;", Enumerable.Range(0, gate == "NOT" ? 1 : 2).Select(x => (char)(x + 'a')))}:std_logic; y: out std_logic);\n" +
                $"end component;\n\n";
        }

        private string GenerateInstance(Gate gate)
        {
            return
                $"    {gate.Name} : {gate.Type}_GATE port map({String.Join(",", Enumerable.Range(0, gate.Signals.Count - 1).Select(x => (char)(x + 'a') + "=>" + gate.Signals[x].Name))}, y => {gate.Signals.Last().Name});\n";
        }


    }
}

