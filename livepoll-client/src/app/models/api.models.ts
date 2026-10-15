export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
}

export interface AuthResponse {
  token: string;
  userName: string;
  email: string;
}

export interface PollOption { id: number; text: string; }

export interface PollQuestion {
  id: number;
  text: string;
  orderIndex: number;
  options: PollOption[];
}

export interface PollSession {
  id: number;
  title: string;
  joinCode: string;
  isActive: boolean;
  createdAt: string;
  questions: PollQuestion[];
}

export interface SessionSummary {
  id: number;
  title: string;
  joinCode: string;
  isActive: boolean;
  createdAt: string;
  questionCount: number;
}

export interface OptionResult {
  optionId: number;
  text: string;
  votes: number;
  percentage: number;
}

export interface QuestionResults {
  questionId: number;
  text: string;
  totalVotes: number;
  options: OptionResult[];
}

export interface SessionResults {
  title: string;
  isActive: boolean;
  questions: QuestionResults[];
}

export interface CreateQuestion { text: string; options: string[]; }
export interface CreateSession { title: string; questions: CreateQuestion[]; }
export interface LoginRequest { userName: string; password: string; }
export interface RegisterRequest { userName: string; email: string; password: string; }