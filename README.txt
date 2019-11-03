Commands to run test in ghdl

ghdl -a file.vhd 
ghdl -a tb_file.vhd
ghdl -e tb_file
ghdl -r tb_file --vcd=file.vcd
gtkwave file.vcd