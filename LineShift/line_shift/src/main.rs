extern crate bigint;

use bigint::U256;
use std::u16;

fn main() {
	let initial_vec = create_bitmap();
	println!("Initial vector: ");
	print_bitmap(&initial_vec);

	let u256_vec = convert_to_u256(&initial_vec);
	println!("Vector as U256: ");
	print_u256_in_rows(&u256_vec);

	let shifted_vec = shift_bitmap(&initial_vec, 1);
	println!("Shifted vector: ");
	print_bitmap(&shifted_vec);
}

// creates bitmap 16x16 with a line of 1s inside
fn create_bitmap() -> Vec<u16> {
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

fn shift_bitmap(vec: &Vec<u16>, shift: u16) -> Vec<u16> {
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
			let mut bit = (row >> bit_idx) & 0b_0000_0000_0000_0001_u16;
			if bit != 0 {
				bit = 1u16;
			}
			print!("{} ", bit);
		}
		println!();
	}
}

fn print_u256(vec: &U256) {
	for i in 0..256 {
		let bit = vec.bit(i);
		if bit {
			print!("1 ");
		} else {
			print!("0 ");
		}
	}
	println!("")
}

fn print_u256_in_rows(vec: &U256) {
	let row_max_idx = 16;
	let col_max_idx = 16;
	for i in 0..row_max_idx {
		for j in 0..col_max_idx {
			let idx = i * col_max_idx + j;
			let bit = vec.bit(idx);
			if bit {
				print!("1 ");
			} else {
				print!("0 ");
			}
		}
		println!("");
	}
	println!("")
}

fn convert_to_u256(vec: &Vec<u16>) -> U256 {
	let mut result = U256::from(0x00);
	let mut idx = 0;
	for row in vec {
		let mut item = U256::from(*row);
		item = item << 16 * idx;
		result = result | item;
		idx = idx + 1;
	}
	result
}
