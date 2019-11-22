use super::bitvector::BitVector;

#[derive(Default, Debug, Clone)]
pub struct Context {
	memory: Vec<(BitVector, u32)>,
}
