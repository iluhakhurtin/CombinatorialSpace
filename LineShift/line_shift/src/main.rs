extern crate bigint;
extern crate bit_vec;

use bigint::U256;
use bit_vec::BitVec;
use std::mem;

fn main() {
	let initial_vec = create_bitmap_line();
	println!();
	print_bitmap(&initial_vec);

	let shifted_vec = shift_bitmap_line(&initial_vec, 1);
	println!();
	print_bitmap(&shifted_vec);

	convert_to_bit_vec(&initial_vec);
}

// creates bitmap 16x16 with a line of 1s inside
fn create_bitmap_line() -> Vec<u16> {
	let vec = vec![
		0b_0000_0000_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0110_0000_0000,
		0b_0000_0000_0000_0000,
	];
	vec
}

fn shift_bitmap_line(vec: &Vec<u16>, shift: u16) -> Vec<u16> {
	let mut result = Vec::with_capacity(vec.len());
	for row in vec {
		let new_row = row >> shift;
		result.push(new_row);
	}
	result
}

fn print_bitmap(vec: &Vec<u16>) {
	for row in vec {
		for bit_idx in 0..16 {
			let mut bit = (row << bit_idx) & 0b_1000_0000_0000_0000_u16;
			if bit != 0 {
				bit = 1u16;
			}
			print!("{} ", bit);
		}
		println!();
	}
}

fn print_bit_vec(bit_vec: &BitVec) {
	for bit in bit_vec {
		print!("{}", bit as u8);
	}
}

fn convert_to_bytes(vec: &Vec<u16>) -> Vec<u8> {
	let capacity = 2 * vec.len();
	let mut byte_vec = Vec::with_capacity(capacity);
	for row in vec {
		let bytes = row.to_be_bytes();
		byte_vec.push(bytes[0]);
		byte_vec.push(bytes[1]);
	}
	byte_vec
}

fn convert_to_bit_vec(vec: &Vec<u16>) {
	let byte_vec = convert_to_bytes(&vec);
	let mut bit_vec = BitVec::from_bytes(&byte_vec);
	print_bit_vec(&bit_vec);
	println!();
	println!("bit vec size: {}", bit_vec.len());
}
