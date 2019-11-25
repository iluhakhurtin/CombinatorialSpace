use super::bitvector::BitVector;
use ndarray::{s, Array3};
use rand::Rng;

// (φ, y, x) -> u128
type CodeSpace = Array3<BitVector>;

/// Creates a 3 dimentional array 10x10x10 of 128 bits vectors.
/// Every vector can have up to 27 randomly set bits.
/// However, the main trait of the space is that the codes
/// are changed smoothly if the coordinates are chnaged smoothly as well.
/// In other words there is a high correlation between the codes next to each other.
/// Also the space is closed up (looped) in the first dimention.
fn generate_code_space() -> CodeSpace {
	const MAX_A: usize = 10;
	const MAX_X: usize = MAX_A;
	const MAX_Y: usize = MAX_A;
	const LINEAR_AMEND: usize = 2; //this is amendment for coordinates that are not closed (looped)
															 //because seeds space in these dimentions must be a bit bigger for appropriate code generation

	//3 dimentinal seeds space with 1 random set bit in every point
	let seeds = CodeSpace::from_shape_fn((MAX_A, MAX_Y + LINEAR_AMEND, MAX_X + LINEAR_AMEND), |_| {
		BitVector::random(1)
	});

	//3 dimentional space with closed (looped) a axis which has 10 possible values from 0 to 9, but the next
	//after 9 is again 0 and it is infinitive
	//A code space has a bit vector in every point structured on top of seeds.
	//It combines 27 seeds (subspace 3x3x3) which lay in the vicinity of one point. Despite the fact that a bit in every seed is
	//set randomly, the 3d unbroken subspace always has the same vector if all the bits are combined by logical OR.
	//As a result, such a logic guarantees smoth change for every summarizing vector's bits if the subspace is moved
	//in a vicinity of a point. Thus, a code space vector (i.e. code) has up to 27 set to 1 bits but can have less if some seeds points have matching bits
	//which are changed smoothly in correspondence to the distance from each other.
	//In other words, this logic creates a 3d codes space every code in which has up to 27 bits changing smoothly
	//in relation to the coordinates of this space. One of the axis in the space (a) is looped and can have values
	//from 0 to 9.
	//More than 750 codes in the code space contains from 15 to 18 set bits after such initialization.

	let code_space = CodeSpace::from_shape_fn((MAX_A, MAX_Y, MAX_X), |(a, y, x)| {
		let (a, y, x) = (a as isize, 1 + y as isize, 1 + x as isize);

		match a {
			0 => seeds
				.slice(s![-1.., y - 1..=y + 1, x - 1..=x + 1])
				.into_iter()
				.chain(
					seeds
						.slice(s![..=1, y - 1..=y + 1, x - 1..=x + 1])
						.into_iter(),
				)
				.fold(BitVector::default(), |a, &b| a | b),

			9 => seeds
				.slice(s![-2.., y - 1..=y + 1, x - 1..=x + 1])
				.into_iter()
				.chain(
					seeds
						.slice(s![..=0, y - 1..=y + 1, x - 1..=x + 1])
						.into_iter(),
				)
				.fold(BitVector::default(), |a, &b| a | b),

			_ => seeds
				.slice(s![a..=a + 1, y - 1..=y + 1, x - 1..=x + 1])
				.into_iter()
				.fold(BitVector::default(), |a, &b| a | b),
		}
	});

	code_space
}

#[cfg(test)]
mod tests {
	use super::*;
	use crate::diffspace::analytics::correlation;

	#[test]
	fn test_generate_code_space() {
		let code_space = generate_code_space();

		assert!(
			dbg!(correlation(
				code_space[[1, 1, 1]].value(),
				code_space[[1, 1, 2]].value()
			)) > 0.5
		);
		assert!(
			dbg!(correlation(
				code_space[[0, 0, 0]].value(),
				code_space[[9, 0, 0]].value()
			)) > 0.5
		);
		assert!(
			dbg!(correlation(
				code_space[[1, 1, 1]].value(),
				code_space[[6, 6, 6]].value()
			)) < 0.5
		);
	}
}
