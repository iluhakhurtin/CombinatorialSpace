use super::analytics;
use super::bitvector::BitVector;
use super::context_memory_item::ContextMemoryItem;

#[derive(Default, Debug, Clone)]
pub struct Context {
	pub memory: Vec<ContextMemoryItem>,
}

impl Context {
	pub fn covariance(&self, &code: &BitVector) -> f32 {
		let covariance = self
			.memory
			.iter()
			.map(|memory_item| {
				let correlation = analytics::correlation(&memory_item.code.value(), &code.value());
				let full_correlation = memory_item.hits as f32 * correlation;
				full_correlation
			})
			.sum();

		if covariance == 0f32 {
			let result = Context::get_min_covariance();
			return result;
		}
		covariance
	}

	fn get_min_covariance() -> f32 {
		0.0001
	}
}
