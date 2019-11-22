use rand::Rng;

#[derive(
	Debug, Default, Copy, Clone, Hash, Binary, From, Not, BitAnd, BitOr, BitXor, Eq, PartialEq,
)]
pub struct BitVector(pub u128);

impl BitVector {
	pub fn value(&self) -> u128 {
		self.0
	}

	pub fn set(&mut self, index: usize, value: bool) {
		if value {
			self.0 |= 1 << index
		} else {
			self.0 &= !(1 << index)
		}
	}

	pub fn random(saturation: u8) -> BitVector {
		assert!(saturation <= 128);

		let mut result = 0u128;
		let mut rng = rand::thread_rng();

		while result.count_ones() < u32::from(saturation) {
			result |= 1u128 << rng.gen_range(0u8, 127u8);
		}

		BitVector(result)
	}
}

static TRUE: bool = true;
static FALSE: bool = false;

impl std::ops::Index<usize> for BitVector {
	type Output = bool;

	fn index(&self, index: usize) -> &bool {
		if (self.0 & 1 << index) != 0 {
			&TRUE
		} else {
			&FALSE
		}
	}
}

#[test]
fn random() {
	let vec = BitVector::random(15);
	println!("{:0128b}", vec);
	assert_eq!(vec.value().count_ones(), 15);
}
