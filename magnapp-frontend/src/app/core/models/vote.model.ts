export interface Vote {
  userId: string;
  value: VoteValue;
  timestamp: Date;
}

export type VoteValue = 1 | 2 | 3 | 5 | 8 | 13 | 21 | 'coffee';

export interface VoteStatistics {
  average: number | null;
  totalVotes: number;
  consensus: boolean;
  distribution: { [key in VoteValue]?: number };
  excludedCoffeeVotes: number;
}
