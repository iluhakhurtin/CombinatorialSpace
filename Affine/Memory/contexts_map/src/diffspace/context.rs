use super::bitvector::BitVector;

#[derive(Default, Debug, Clone)]
pub struct Context {
	pub memory: Vec<(BitVector, u32)>,
}
