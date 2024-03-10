use std::fs::File;
use std::path::Path;

use crate::Context;
use crate::Transformation;
use crate::Information;

extern crate rayon;
use rayon::prelude::*;

extern crate num_traits;
use num_traits::int::PrimInt;

extern crate serde;
use serde::{Serialize, Deserialize};

extern crate ordered_float;
use ordered_float::OrderedFloat;

/// The context space.
#[derive(Serialize, Deserialize)]
pub struct ContextSpace<T: PrimInt + Sync + Send + Serialize> {
    pub interpretations: Vec<Information<T>>,
    pub contexts: Vec<Context<T>>
}

impl<T: PrimInt + Sync + Send + Serialize> ContextSpace<T> {
    /// This constructor makes the context space without
    /// predefined contexts.
    pub fn new() -> ContextSpace<T> {
        let contexts = Vec::<Context<T>>::new();
        let interpretations = Vec::<Information<T>>::new();

        ContextSpace { contexts, interpretations }
    }

    pub fn len(&self) -> usize {
        let len = &self.contexts.len();
        *len
    }

    /// During supervised learning the system is given the known transformation
    /// because the human brain 'knows' what the movement the eye does 
    /// and a pair of the information i and its interpretation i_int
    pub fn learn(&mut self, t: &Transformation, i: &Information<T>, int: Information<T>) {
        // teach contexts
        let d = 0.01; // learning distance
        
        if !self.contexts
            .par_iter()
            .any(|c| c.tran.distance_to(t) <= d) {

            let c = Context::<T>::with_transformation(t.clone());
            self.contexts.push(c);
        }

        self.contexts
            .par_iter_mut()
            .filter(|c| c.tran.distance_to(t) <= d)
            .for_each(|c| c.learn(i, &int));

        self.add_interpretation(int);
    }

    fn add_interpretation(&mut self, int: Information<T>) {
        if !self.interpretations
            .par_iter()
            .any(|i| *i == int) {
                self.interpretations.push(int);
            }
    }

    /// The method tries to find the best interpretation among the contexts 
    /// and if it is higher than the provided accuracy returns it with
    /// the particular accuracy.
    /// The method uses probability dependent random selection logic that 
    /// does not guarantees.
    /// Returns interpretation, its transformation and the probability of it.
    pub fn interpret(&self, i: &Information<T>, accuracy: f32) 
        -> Option<(Information<T>, Transformation, f32, Information<T>)> {
        // request the interpretation from every context and select only the contexts
        // which interpretation accuracy is higher than the required 
        let c_int_acc = self.contexts.par_iter()
            .filter_map(|c| {
                match c.interpret(i) {
                    None => None,
                    Some((actual_int, actual_int_accuracy)) => {
                        if actual_int_accuracy >= accuracy {
                            // try to find interpretation among already seen
                            match self.find_existing_interpretation(&actual_int, accuracy) {
                                None => None,
                                Some((existing_int, existing_int_accuracy)) => {
                                    let full_accuracy = actual_int_accuracy * existing_int_accuracy;
                                    return Some((c, existing_int, OrderedFloat(full_accuracy), actual_int))
                                }
                            }
                        }
                        else{
                            None
                        }
                    }
                }
            })
            .max_by_key(|c_int_acc| c_int_acc.2);

        match c_int_acc {
            None => None,
            Some(val) => {
                let tran = val.0.tran.clone();
                let existing_int = val.1.clone();
                let accuracy = val.2.into_inner();
                let actual_int = val.3;

                return Some((existing_int, tran, accuracy, actual_int));
            }
        }
    }

    /// Looks through the existing interpretations to find the one which 
    /// 1) looks like target_int with the highest accuracy
    /// 2) the accuracy is not lower than the given one
    fn find_existing_interpretation(&self, target_int: &Information<T>, accuracy: f32) -> Option<(Information<T>, f32)> {
        let existing_int = &self.interpretations
            .par_iter()
            .filter_map(|int| match target_int.coherence_to(int) {
                Ok(coherence) => {
                    if coherence >= accuracy {
                        return Some((int, OrderedFloat(coherence)))
                    }
                    None
                },
                Err(_) => {
                    None
                }
            })
            .max_by_key(|int_data| int_data.1);

        return match existing_int {
            None => None,
            Some(int_data) => {
                let coherence = int_data.1.into_inner();
                let data = int_data.0.data.clone();
                let name = int_data.0.name.clone();
                let int_clone = Information { data, name };
                Some((int_clone, coherence))
            }
        }
    }

    pub fn save<P: AsRef<Path>>(&self, path: P) -> Result<(), std::io::Error> {
        let f = File::create(path)?;
        bincode::serialize_into(f, self).unwrap();
        return Ok(());
    }

    pub fn load<P: AsRef<Path>>(path: P) -> Result<ContextSpace<T>, std::io::Error> 
        where ContextSpace<T>: for<'de> Deserialize<'de> {
        let f = File::open(path)?;
        let cs = bincode::deserialize_from(f).unwrap();
        return Ok(cs);
    }
}

#[test]
fn can_save_and_load() {
    let mut path = std::path::PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    path.push("files/tests/context_space/cs_9x9.bin");
    
    let expected = ContextSpace::<u8>::new();
    expected.save(&path).unwrap();

    let actual = ContextSpace::<u8>::load(path).unwrap();

    assert_eq!(actual.len(), expected.len());
}