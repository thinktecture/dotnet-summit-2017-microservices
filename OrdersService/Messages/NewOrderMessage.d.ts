declare module server {
	interface newOrderMessage {
		order: {
			id: any;
			dateTime: Date;
			items: any[];
		};
		userId: string;
	}
}
